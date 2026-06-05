#include "detector.h"

#include "autoencoder.h"
#include "csv_loader.h"
#include "logger.h"
#include "preprocess.h"

#include <errno.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#ifdef _WIN32
#include <direct.h>
#define MAKE_DIR(path) _mkdir(path)
#else
#include <sys/stat.h>
#define MAKE_DIR(path) mkdir(path, 0755)
#endif

#define MODEL_PATH "model/autoencoder_model.bin"
#define NORMALIZER_PATH "model/normalize_params.txt"
#define TRAIN_LOG_PATH "output/train_log.txt"
#define DETECT_RESULT_PATH "output/detect_result.csv"
#define ALARM_LOG_PATH "output/alarm_log.txt"
#define TEST_METRICS_PATH "output/test_metrics.txt"
#define DEFAULT_EPOCHS 80
#define DEFAULT_LR 0.005
#define THRESHOLD_PERCENTILE 0.95

typedef struct {
    int tp;
    int tn;
    int fp;
    int fn;
} Metrics;

static void ensure_directory(const char *path)
{
    if (MAKE_DIR(path) != 0 && errno != EEXIST) {
        fprintf(stderr, "Warning: failed to create directory %s\n", path);
        logger_error("failed to create directory: %s", path);
    } else {
        logger_info("directory ready: %s", path);
    }
}

static void ensure_project_dirs(void)
{
    ensure_directory("model");
    ensure_directory("output");
    ensure_directory("build");
}

static void print_metrics(const Metrics *metrics)
{
    int total = metrics->tp + metrics->tn + metrics->fp + metrics->fn;
    double accuracy = total > 0 ? (double)(metrics->tp + metrics->tn) / (double)total : 0.0;
    double precision = (metrics->tp + metrics->fp) > 0 ?
        (double)metrics->tp / (double)(metrics->tp + metrics->fp) : 0.0;
    double recall = (metrics->tp + metrics->fn) > 0 ?
        (double)metrics->tp / (double)(metrics->tp + metrics->fn) : 0.0;
    double f1 = (precision + recall) > 0.0 ?
        2.0 * precision * recall / (precision + recall) : 0.0;

    printf("\nConfusion Matrix:\n");
    printf("TP=%d TN=%d FP=%d FN=%d\n", metrics->tp, metrics->tn, metrics->fp, metrics->fn);
    printf("Accuracy=%.4f Precision=%.4f Recall=%.4f F1=%.4f\n",
           accuracy, precision, recall, f1);
}

static void save_metrics(const Metrics *metrics)
{
    FILE *fp = fopen(TEST_METRICS_PATH, "w");
    int total = metrics->tp + metrics->tn + metrics->fp + metrics->fn;
    double accuracy = total > 0 ? (double)(metrics->tp + metrics->tn) / (double)total : 0.0;
    double precision = (metrics->tp + metrics->fp) > 0 ?
        (double)metrics->tp / (double)(metrics->tp + metrics->fp) : 0.0;
    double recall = (metrics->tp + metrics->fn) > 0 ?
        (double)metrics->tp / (double)(metrics->tp + metrics->fn) : 0.0;
    double f1 = (precision + recall) > 0.0 ?
        2.0 * precision * recall / (precision + recall) : 0.0;

    if (fp == NULL) {
        logger_error("failed to save metrics: %s", TEST_METRICS_PATH);
        return;
    }

    fprintf(fp, "TP=%d\n", metrics->tp);
    fprintf(fp, "TN=%d\n", metrics->tn);
    fprintf(fp, "FP=%d\n", metrics->fp);
    fprintf(fp, "FN=%d\n", metrics->fn);
    fprintf(fp, "Accuracy=%.4f\n", accuracy);
    fprintf(fp, "Precision=%.4f\n", precision);
    fprintf(fp, "Recall=%.4f\n", recall);
    fprintf(fp, "F1=%.4f\n", f1);
    fclose(fp);
}

static int write_detection_outputs(const Dataset *dataset, const Autoencoder *model, int print_each_row, int with_metrics)
{
    FILE *result_fp = fopen(DETECT_RESULT_PATH, "w");
    FILE *alarm_fp = fopen(ALARM_LOG_PATH, "w");
    Metrics metrics = {0, 0, 0, 0};
    int alarm_count = 0;

    if (result_fp == NULL || alarm_fp == NULL) {
        fprintf(stderr, "Failed to open output files.\n");
        logger_error("failed to open output files: %s or %s", DETECT_RESULT_PATH, ALARM_LOG_PATH);
        if (result_fp != NULL) {
            fclose(result_fp);
        }
        if (alarm_fp != NULL) {
            fclose(alarm_fp);
        }
        return 0;
    }

    fprintf(result_fp, "record,error,threshold,predicted,label,status\n");
    fprintf(alarm_fp, "record,error,threshold,label\n");

    for (int i = 0; i < dataset->row_count; i++) {
        const double *row = &dataset->features[(size_t)i * (size_t)dataset->feature_count];
        double error = ae_reconstruct_error(model, row);
        int predicted = error > model->threshold ? 1 : 0;
        int label = dataset->labels != NULL ? dataset->labels[i] : -1;
        const char *status = predicted ? "ANOMALY" : "NORMAL";

        fprintf(result_fp, "%d,%.10f,%.10f,%d,%d,%s\n",
                i + 1, error, model->threshold, predicted, label, status);

        if (predicted) {
            alarm_count++;
            fprintf(alarm_fp, "%d,%.10f,%.10f,%d\n", i + 1, error, model->threshold, label);
            printf("[ALARM] record=%d error=%.6f threshold=%.6f status=ANOMALY\n",
                   i + 1, error, model->threshold);
        } else if (print_each_row) {
            printf("[OK] record=%d error=%.6f threshold=%.6f status=NORMAL\n",
                   i + 1, error, model->threshold);
        }

        if (with_metrics && dataset->has_label && label >= 0) {
            if (predicted == 1 && label == 1) {
                metrics.tp++;
            } else if (predicted == 0 && label == 0) {
                metrics.tn++;
            } else if (predicted == 1 && label == 0) {
                metrics.fp++;
            } else if (predicted == 0 && label == 1) {
                metrics.fn++;
            }
        }
    }

    fclose(result_fp);
    fclose(alarm_fp);

    printf("Detection result saved to %s\n", DETECT_RESULT_PATH);
    printf("Alarm log saved to %s\n", ALARM_LOG_PATH);
    logger_info("detection completed rows=%d alarms=%d result=%s alarm_log=%s",
                dataset->row_count, alarm_count, DETECT_RESULT_PATH, ALARM_LOG_PATH);

    if (with_metrics && dataset->has_label) {
        print_metrics(&metrics);
        save_metrics(&metrics);
        logger_info("metrics TP=%d TN=%d FP=%d FN=%d",
                    metrics.tp, metrics.tn, metrics.fp, metrics.fn);
    } else if (with_metrics) {
        printf("No label column found; metrics were not calculated.\n");
        logger_error("metrics skipped because label column was not found");
    }

    return 1;
}

int run_train(const char *csv_path)
{
    Dataset train_data;
    Normalizer normalizer;
    Autoencoder model;
    int ok = 0;

    ensure_project_dirs();
    logger_info("train started csv=%s", csv_path);

    if (!csv_load_dataset(csv_path, 1, &train_data)) {
        logger_error("failed to load train csv: %s", csv_path);
        return 1;
    }
    logger_info("train csv loaded rows=%d features=%d has_label=%d",
                train_data.row_count, train_data.feature_count, train_data.has_label);

    normalizer_fit(&train_data, &normalizer);
    normalizer_apply(&train_data, &normalizer);

    if (!normalizer_save(NORMALIZER_PATH, &normalizer)) {
        logger_error("failed to save normalizer: %s", NORMALIZER_PATH);
        dataset_free(&train_data);
        return 1;
    }
    logger_info("normalizer saved: %s", NORMALIZER_PATH);

    if (!ae_init(&model, train_data.feature_count, 20260605U)) {
        fprintf(stderr, "Failed to initialize autoencoder.\n");
        logger_error("failed to initialize autoencoder");
        dataset_free(&train_data);
        return 1;
    }

    printf("Start training autoencoder: rows=%d features=%d epochs=%d lr=%.4f\n",
           train_data.row_count, train_data.feature_count, DEFAULT_EPOCHS, DEFAULT_LR);

    if (!ae_train(&model, train_data.features, train_data.row_count, DEFAULT_EPOCHS, DEFAULT_LR, TRAIN_LOG_PATH)) {
        logger_error("training failed");
        goto cleanup;
    }

    model.threshold = ae_percentile_error(&model, train_data.features, train_data.row_count, THRESHOLD_PERCENTILE);
    printf("Threshold percentile=%.2f threshold=%.10f\n", THRESHOLD_PERCENTILE, model.threshold);
    logger_info("threshold percentile=%.2f threshold=%.10f", THRESHOLD_PERCENTILE, model.threshold);

    if (!ae_save(MODEL_PATH, &model)) {
        logger_error("failed to save model: %s", MODEL_PATH);
        goto cleanup;
    }

    printf("Model saved to %s\n", MODEL_PATH);
    printf("Normalizer saved to %s\n", NORMALIZER_PATH);
    printf("Train log saved to %s\n", TRAIN_LOG_PATH);
    logger_info("train completed model=%s normalizer=%s train_log=%s",
                MODEL_PATH, NORMALIZER_PATH, TRAIN_LOG_PATH);
    ok = 1;

cleanup:
    ae_free(&model);
    dataset_free(&train_data);
    return ok ? 0 : 1;
}

int run_test(const char *csv_path)
{
    Dataset test_data;
    Normalizer normalizer;
    Autoencoder model;
    int ok = 0;

    ensure_project_dirs();
    logger_info("test started csv=%s", csv_path);

    if (!normalizer_load(NORMALIZER_PATH, &normalizer)) {
        logger_error("failed to load normalizer: %s", NORMALIZER_PATH);
        return 1;
    }
    logger_info("normalizer loaded: %s", NORMALIZER_PATH);
    if (!ae_load(MODEL_PATH, &model)) {
        logger_error("failed to load model: %s", MODEL_PATH);
        return 1;
    }
    logger_info("model loaded: %s threshold=%.10f", MODEL_PATH, model.threshold);
    if (!csv_load_dataset(csv_path, 0, &test_data)) {
        logger_error("failed to load test csv: %s", csv_path);
        ae_free(&model);
        return 1;
    }
    logger_info("test csv loaded rows=%d features=%d has_label=%d",
                test_data.row_count, test_data.feature_count, test_data.has_label);

    if (test_data.feature_count != model.input_dim || test_data.feature_count != normalizer.feature_count) {
        fprintf(stderr, "Feature count mismatch.\n");
        logger_error("feature count mismatch data=%d model=%d normalizer=%d",
                     test_data.feature_count, model.input_dim, normalizer.feature_count);
        goto cleanup;
    }

    normalizer_apply(&test_data, &normalizer);
    ok = write_detection_outputs(&test_data, &model, 0, 1);

cleanup:
    dataset_free(&test_data);
    ae_free(&model);
    return ok ? 0 : 1;
}

int run_detect(const char *csv_path)
{
    Dataset input_data;
    Normalizer normalizer;
    Autoencoder model;
    int ok = 0;

    ensure_project_dirs();
    logger_info("detect started csv=%s", csv_path);

    if (!normalizer_load(NORMALIZER_PATH, &normalizer)) {
        logger_error("failed to load normalizer: %s", NORMALIZER_PATH);
        return 1;
    }
    logger_info("normalizer loaded: %s", NORMALIZER_PATH);
    if (!ae_load(MODEL_PATH, &model)) {
        logger_error("failed to load model: %s", MODEL_PATH);
        return 1;
    }
    logger_info("model loaded: %s threshold=%.10f", MODEL_PATH, model.threshold);
    if (!csv_load_dataset(csv_path, 0, &input_data)) {
        logger_error("failed to load detect csv: %s", csv_path);
        ae_free(&model);
        return 1;
    }
    logger_info("detect csv loaded rows=%d features=%d has_label=%d",
                input_data.row_count, input_data.feature_count, input_data.has_label);

    if (input_data.feature_count != model.input_dim || input_data.feature_count != normalizer.feature_count) {
        fprintf(stderr, "Feature count mismatch.\n");
        logger_error("feature count mismatch data=%d model=%d normalizer=%d",
                     input_data.feature_count, model.input_dim, normalizer.feature_count);
        goto cleanup;
    }

    normalizer_apply(&input_data, &normalizer);
    ok = write_detection_outputs(&input_data, &model, 1, 0);

cleanup:
    dataset_free(&input_data);
    ae_free(&model);
    return ok ? 0 : 1;
}
