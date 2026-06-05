#include "preprocess.h"

#include <float.h>
#include <stdio.h>

void normalizer_fit(const Dataset *dataset, Normalizer *normalizer)
{
    normalizer->feature_count = dataset->feature_count;
    for (int j = 0; j < dataset->feature_count; j++) {
        normalizer->min_values[j] = DBL_MAX;
        normalizer->max_values[j] = -DBL_MAX;
    }

    for (int i = 0; i < dataset->row_count; i++) {
        const double *row = &dataset->features[(size_t)i * (size_t)dataset->feature_count];
        for (int j = 0; j < dataset->feature_count; j++) {
            if (row[j] < normalizer->min_values[j]) {
                normalizer->min_values[j] = row[j];
            }
            if (row[j] > normalizer->max_values[j]) {
                normalizer->max_values[j] = row[j];
            }
        }
    }
}

void normalizer_apply(Dataset *dataset, const Normalizer *normalizer)
{
    for (int i = 0; i < dataset->row_count; i++) {
        double *row = &dataset->features[(size_t)i * (size_t)dataset->feature_count];
        for (int j = 0; j < dataset->feature_count; j++) {
            double min_value = normalizer->min_values[j];
            double max_value = normalizer->max_values[j];
            double range = max_value - min_value;
            double value = 0.0;

            if (range != 0.0) {
                value = (row[j] - min_value) / range;
            }

            if (value < 0.0) {
                value = 0.0;
            } else if (value > 1.0) {
                value = 1.0;
            }

            row[j] = value;
        }
    }
}

int normalizer_save(const char *path, const Normalizer *normalizer)
{
    FILE *fp = fopen(path, "w");
    if (fp == NULL) {
        fprintf(stderr, "Failed to save normalizer: %s\n", path);
        return 0;
    }

    fprintf(fp, "%d\n", normalizer->feature_count);
    for (int i = 0; i < normalizer->feature_count; i++) {
        fprintf(fp, "%.17g %.17g\n", normalizer->min_values[i], normalizer->max_values[i]);
    }

    fclose(fp);
    return 1;
}

int normalizer_load(const char *path, Normalizer *normalizer)
{
    FILE *fp = fopen(path, "r");
    if (fp == NULL) {
        fprintf(stderr, "Failed to load normalizer: %s\n", path);
        return 0;
    }

    if (fscanf(fp, "%d", &normalizer->feature_count) != 1 ||
        normalizer->feature_count <= 0 ||
        normalizer->feature_count > MAX_FEATURES) {
        fprintf(stderr, "Invalid normalizer file: %s\n", path);
        fclose(fp);
        return 0;
    }

    for (int i = 0; i < normalizer->feature_count; i++) {
        if (fscanf(fp, "%lf %lf", &normalizer->min_values[i], &normalizer->max_values[i]) != 2) {
            fprintf(stderr, "Invalid normalizer values in %s\n", path);
            fclose(fp);
            return 0;
        }
    }

    fclose(fp);
    return 1;
}
