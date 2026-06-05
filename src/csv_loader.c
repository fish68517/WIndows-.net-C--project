#include "csv_loader.h"
#include "logger.h"

#include <ctype.h>
#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define MAX_LINE_LENGTH 16384
#define UNSW_RAW_FIELD_COUNT 45

const char *FEATURE_NAMES[FEATURE_COUNT] = {
    "dur", "spkts", "dpkts", "sbytes", "dbytes", "rate", "sttl", "dttl",
    "sload", "dload", "sloss", "dloss", "sinpkt", "dinpkt", "sjit", "djit"
};

static const int UNSW_RAW_FEATURE_COLUMNS[FEATURE_COUNT] = {
    1, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19
};

static const int UNSW_RAW_LABEL_COLUMN = 44;

static void dataset_init(Dataset *dataset)
{
    dataset->features = NULL;
    dataset->labels = NULL;
    dataset->row_count = 0;
    dataset->feature_count = FEATURE_COUNT;
    dataset->has_label = 0;
}

void dataset_free(Dataset *dataset)
{
    if (dataset == NULL) {
        return;
    }
    free(dataset->features);
    free(dataset->labels);
    dataset_init(dataset);
}

static char *trim_field(char *value)
{
    char *end;
    size_t len;

    if (value == NULL) {
        return value;
    }

    if ((unsigned char)value[0] == 0xEF &&
        (unsigned char)value[1] == 0xBB &&
        (unsigned char)value[2] == 0xBF) {
        value += 3;
    }

    while (*value != '\0' && isspace((unsigned char)*value)) {
        value++;
    }

    end = value + strlen(value);
    while (end > value && isspace((unsigned char)*(end - 1))) {
        end--;
    }
    *end = '\0';

    len = strlen(value);
    if (len >= 2 && value[0] == '"' && value[len - 1] == '"') {
        value[len - 1] = '\0';
        value++;
    }

    return value;
}

static int split_delimited_line(char *line, char **fields, int max_fields, char delimiter)
{
    int count = 0;
    int in_quotes = 0;
    char *p = line;

    if (line == NULL || fields == NULL || max_fields <= 0) {
        return 0;
    }

    fields[count++] = line;
    while (*p != '\0') {
        if (*p == '"') {
            in_quotes = !in_quotes;
        } else if (*p == delimiter && !in_quotes) {
            *p = '\0';
            if (count >= max_fields) {
                break;
            }
            fields[count++] = p + 1;
        }
        p++;
    }

    for (int i = 0; i < count; i++) {
        fields[i] = trim_field(fields[i]);
    }

    return count;
}

static int split_csv_line(char *line, char **fields, int max_fields)
{
    return split_delimited_line(line, fields, max_fields, ',');
}

static int split_tsv_line(char *line, char **fields, int max_fields)
{
    return split_delimited_line(line, fields, max_fields, '\t');
}

static int equals_ignore_case(const char *a, const char *b)
{
    while (*a != '\0' && *b != '\0') {
        if (tolower((unsigned char)*a) != tolower((unsigned char)*b)) {
            return 0;
        }
        a++;
        b++;
    }
    return *a == '\0' && *b == '\0';
}

static int find_column(char **fields, int field_count, const char *name)
{
    for (int i = 0; i < field_count; i++) {
        if (equals_ignore_case(fields[i], name)) {
            return i;
        }
    }
    return -1;
}

static int parse_double_value(const char *text, double *out)
{
    char *end = NULL;

    if (text == NULL || *text == '\0') {
        return 0;
    }

    errno = 0;
    *out = strtod(text, &end);
    if (errno != 0 || end == text) {
        return 0;
    }

    while (*end != '\0') {
        if (!isspace((unsigned char)*end)) {
            return 0;
        }
        end++;
    }

    return 1;
}

static int parse_int_value(const char *text, int *out)
{
    double value = 0.0;

    if (!parse_double_value(text, &value)) {
        return 0;
    }

    *out = (int)value;
    return 1;
}

static int append_row(Dataset *dataset, const double *values, int label)
{
    int new_count = dataset->row_count + 1;
    double *new_features;
    int *new_labels;

    new_features = (double *)realloc(
        dataset->features,
        (size_t)new_count * (size_t)dataset->feature_count * sizeof(double));
    if (new_features == NULL) {
        return 0;
    }
    dataset->features = new_features;

    new_labels = (int *)realloc(dataset->labels, (size_t)new_count * sizeof(int));
    if (new_labels == NULL) {
        return 0;
    }
    dataset->labels = new_labels;

    memcpy(&dataset->features[(size_t)dataset->row_count * (size_t)dataset->feature_count],
           values,
           (size_t)dataset->feature_count * sizeof(double));
    dataset->labels[dataset->row_count] = label;
    dataset->row_count = new_count;
    return 1;
}

static int parse_row_values(
    char **fields,
    int field_count,
    const int *feature_columns,
    int label_column,
    int has_label,
    int normal_only,
    double *values,
    int *label)
{
    int parsed_label = -1;

    if (has_label && label_column < field_count) {
        if (!parse_int_value(fields[label_column], &parsed_label)) {
            parsed_label = -1;
        }
    }

    if (normal_only && has_label && parsed_label != 0) {
        return 0;
    }

    for (int i = 0; i < FEATURE_COUNT; i++) {
        int column = feature_columns[i];
        if (column >= field_count || !parse_double_value(fields[column], &values[i])) {
            return -1;
        }
    }

    *label = parsed_label;
    return 1;
}

int csv_load_dataset(const char *path, int normal_only, Dataset *dataset)
{
    FILE *fp;
    char line[MAX_LINE_LENGTH];
    char *fields[MAX_CSV_FIELDS];
    int field_count;
    int feature_columns[FEATURE_COUNT];
    int label_column = -1;
    int raw_no_header = 0;
    int delimiter_is_tab = 0;
    int skipped_rows = 0;
    int line_number = 1;

    dataset_init(dataset);

    fp = fopen(path, "r");
    if (fp == NULL) {
        fprintf(stderr, "Failed to open CSV file: %s\n", path);
        return 0;
    }

    if (fgets(line, sizeof(line), fp) == NULL) {
        fprintf(stderr, "CSV file is empty: %s\n", path);
        fclose(fp);
        return 0;
    }

    field_count = split_csv_line(line, fields, MAX_CSV_FIELDS);
    for (int i = 0; i < FEATURE_COUNT; i++) {
        feature_columns[i] = find_column(fields, field_count, FEATURE_NAMES[i]);
    }

    if (feature_columns[0] < 0) {
        field_count = split_tsv_line(line, fields, MAX_CSV_FIELDS);
        if (field_count >= UNSW_RAW_FIELD_COUNT) {
            raw_no_header = 1;
            delimiter_is_tab = 1;
            memcpy(feature_columns, UNSW_RAW_FEATURE_COLUMNS, sizeof(UNSW_RAW_FEATURE_COLUMNS));
            label_column = UNSW_RAW_LABEL_COLUMN;
            dataset->has_label = 1;
            printf("Detected raw UNSW-NB15 TSV format without header: %s\n", path);
            logger_info("csv format detected: raw UNSW-NB15 TSV without header path=%s", path);
        } else {
            fprintf(stderr, "Missing required feature column: %s\n", FEATURE_NAMES[0]);
            logger_error("csv load failed: missing required feature column %s path=%s", FEATURE_NAMES[0], path);
            fclose(fp);
            return 0;
        }
    } else {
        for (int i = 0; i < FEATURE_COUNT; i++) {
            if (feature_columns[i] < 0) {
                fprintf(stderr, "Missing required feature column: %s\n", FEATURE_NAMES[i]);
                logger_error("csv load failed: missing required feature column %s path=%s", FEATURE_NAMES[i], path);
                fclose(fp);
                return 0;
            }
        }

        label_column = find_column(fields, field_count, "label");
        dataset->has_label = label_column >= 0;
        logger_info("csv format detected: header CSV path=%s has_label=%d", path, dataset->has_label);
    }

    do {
        double values[FEATURE_COUNT];
        int label = -1;
        int parse_result;

        if (!raw_no_header) {
            if (fgets(line, sizeof(line), fp) == NULL) {
                break;
            }
            line_number++;
        }

        field_count = delimiter_is_tab ?
            split_tsv_line(line, fields, MAX_CSV_FIELDS) :
            split_csv_line(line, fields, MAX_CSV_FIELDS);
        if (field_count <= 1) {
            if (raw_no_header && fgets(line, sizeof(line), fp) != NULL) {
                line_number++;
                continue;
            }
            if (!raw_no_header) {
                continue;
            }
        }

        parse_result = parse_row_values(
            fields,
            field_count,
            feature_columns,
            label_column,
            dataset->has_label,
            normal_only,
            values,
            &label);

        if (parse_result == 0) {
            if (raw_no_header) {
                if (fgets(line, sizeof(line), fp) == NULL) {
                    break;
                }
                line_number++;
                continue;
            }
            continue;
        }

        if (parse_result < 0) {
            skipped_rows++;
            if (raw_no_header) {
                if (fgets(line, sizeof(line), fp) == NULL) {
                    break;
                }
                line_number++;
                continue;
            }
            continue;
        }

        if (!append_row(dataset, values, label)) {
            fprintf(stderr, "Out of memory while loading %s\n", path);
            dataset_free(dataset);
            fclose(fp);
            return 0;
        }

        if (raw_no_header) {
            if (fgets(line, sizeof(line), fp) == NULL) {
                break;
            }
            line_number++;
        }
    } while (raw_no_header || !feof(fp));

    fclose(fp);

    if (dataset->row_count == 0) {
        fprintf(stderr, "No usable rows loaded from %s\n", path);
        dataset_free(dataset);
        return 0;
    }

    if (skipped_rows > 0) {
        fprintf(stderr, "Skipped %d invalid rows while loading %s\n", skipped_rows, path);
        logger_info("csv load skipped invalid rows=%d path=%s", skipped_rows, path);
    }

    printf("Loaded %d rows from %s", dataset->row_count, path);
    if (normal_only && dataset->has_label) {
        printf(" (normal rows only)");
    }
    printf(".\n");
    logger_info("csv load completed rows=%d features=%d has_label=%d normal_only=%d path=%s",
                dataset->row_count,
                dataset->feature_count,
                dataset->has_label,
                normal_only,
                path);

    (void)line_number;
    return 1;
}
