#include "autoencoder.h"

#include <math.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

typedef struct {
    char magic[8];
    int version;
    int input_dim;
    int hidden1;
    int bottleneck;
    int hidden2;
    double threshold;
} ModelHeader;

static double relu(double value)
{
    return value > 0.0 ? value : 0.0;
}

static double relu_grad_from_activation(double activation)
{
    return activation > 0.0 ? 1.0 : 0.0;
}

static double random_weight(int fan_in, int fan_out)
{
    double scale = sqrt(6.0 / (double)(fan_in + fan_out));
    double r = (double)rand() / (double)RAND_MAX;
    return (r * 2.0 - 1.0) * scale;
}

static int write_array(FILE *fp, const double *values, size_t count)
{
    return fwrite(values, sizeof(double), count, fp) == count;
}

static int read_array(FILE *fp, double *values, size_t count)
{
    return fread(values, sizeof(double), count, fp) == count;
}

int ae_init(Autoencoder *model, int input_dim, unsigned int seed)
{
    if (input_dim <= 0 || input_dim > MAX_FEATURES) {
        return 0;
    }

    memset(model, 0, sizeof(*model));
    model->input_dim = input_dim;
    model->threshold = 0.0;

    model->w1 = (double *)malloc((size_t)input_dim * AE_HIDDEN1 * sizeof(double));
    model->b1 = (double *)calloc(AE_HIDDEN1, sizeof(double));
    model->w2 = (double *)malloc((size_t)AE_HIDDEN1 * AE_BOTTLENECK * sizeof(double));
    model->b2 = (double *)calloc(AE_BOTTLENECK, sizeof(double));
    model->w3 = (double *)malloc((size_t)AE_BOTTLENECK * AE_HIDDEN2 * sizeof(double));
    model->b3 = (double *)calloc(AE_HIDDEN2, sizeof(double));
    model->w4 = (double *)malloc((size_t)AE_HIDDEN2 * input_dim * sizeof(double));
    model->b4 = (double *)calloc((size_t)input_dim, sizeof(double));

    if (model->w1 == NULL || model->b1 == NULL || model->w2 == NULL || model->b2 == NULL ||
        model->w3 == NULL || model->b3 == NULL || model->w4 == NULL || model->b4 == NULL) {
        ae_free(model);
        return 0;
    }

    srand(seed);
    for (int i = 0; i < input_dim * AE_HIDDEN1; i++) {
        model->w1[i] = random_weight(input_dim, AE_HIDDEN1);
    }
    for (int i = 0; i < AE_HIDDEN1 * AE_BOTTLENECK; i++) {
        model->w2[i] = random_weight(AE_HIDDEN1, AE_BOTTLENECK);
    }
    for (int i = 0; i < AE_BOTTLENECK * AE_HIDDEN2; i++) {
        model->w3[i] = random_weight(AE_BOTTLENECK, AE_HIDDEN2);
    }
    for (int i = 0; i < AE_HIDDEN2 * input_dim; i++) {
        model->w4[i] = random_weight(AE_HIDDEN2, input_dim);
    }

    return 1;
}

void ae_free(Autoencoder *model)
{
    if (model == NULL) {
        return;
    }
    free(model->w1);
    free(model->b1);
    free(model->w2);
    free(model->b2);
    free(model->w3);
    free(model->b3);
    free(model->w4);
    free(model->b4);
    memset(model, 0, sizeof(*model));
}

static void ae_forward(
    const Autoencoder *model,
    const double *input,
    double *a1,
    double *a2,
    double *a3,
    double *output)
{
    int n = model->input_dim;

    for (int j = 0; j < AE_HIDDEN1; j++) {
        double sum = model->b1[j];
        for (int i = 0; i < n; i++) {
            sum += input[i] * model->w1[(size_t)i * AE_HIDDEN1 + j];
        }
        a1[j] = relu(sum);
    }

    for (int k = 0; k < AE_BOTTLENECK; k++) {
        double sum = model->b2[k];
        for (int j = 0; j < AE_HIDDEN1; j++) {
            sum += a1[j] * model->w2[(size_t)j * AE_BOTTLENECK + k];
        }
        a2[k] = relu(sum);
    }

    for (int j = 0; j < AE_HIDDEN2; j++) {
        double sum = model->b3[j];
        for (int k = 0; k < AE_BOTTLENECK; k++) {
            sum += a2[k] * model->w3[(size_t)k * AE_HIDDEN2 + j];
        }
        a3[j] = relu(sum);
    }

    for (int i = 0; i < n; i++) {
        double sum = model->b4[i];
        for (int j = 0; j < AE_HIDDEN2; j++) {
            sum += a3[j] * model->w4[(size_t)j * n + i];
        }
        output[i] = sum;
    }
}

double ae_reconstruct_error(const Autoencoder *model, const double *input)
{
    double a1[AE_HIDDEN1];
    double a2[AE_BOTTLENECK];
    double a3[AE_HIDDEN2];
    double output[MAX_FEATURES];
    double mse = 0.0;
    int n = model->input_dim;

    ae_forward(model, input, a1, a2, a3, output);

    for (int i = 0; i < n; i++) {
        double diff = output[i] - input[i];
        mse += diff * diff;
    }
    return mse / (double)n;
}

static double ae_train_one(Autoencoder *model, const double *input, double learning_rate)
{
    double a1[AE_HIDDEN1];
    double a2[AE_BOTTLENECK];
    double a3[AE_HIDDEN2];
    double output[MAX_FEATURES];
    double d_out[MAX_FEATURES];
    double d_a3[AE_HIDDEN2];
    double d_z3[AE_HIDDEN2];
    double d_a2[AE_BOTTLENECK];
    double d_z2[AE_BOTTLENECK];
    double d_a1[AE_HIDDEN1];
    double d_z1[AE_HIDDEN1];
    double mse = 0.0;
    int n = model->input_dim;

    ae_forward(model, input, a1, a2, a3, output);

    for (int i = 0; i < n; i++) {
        double diff = output[i] - input[i];
        mse += diff * diff;
        d_out[i] = 2.0 * diff / (double)n;
    }
    mse /= (double)n;

    for (int j = 0; j < AE_HIDDEN2; j++) {
        double sum = 0.0;
        for (int i = 0; i < n; i++) {
            sum += d_out[i] * model->w4[(size_t)j * n + i];
        }
        d_a3[j] = sum;
        d_z3[j] = d_a3[j] * relu_grad_from_activation(a3[j]);
    }

    for (int k = 0; k < AE_BOTTLENECK; k++) {
        double sum = 0.0;
        for (int j = 0; j < AE_HIDDEN2; j++) {
            sum += d_z3[j] * model->w3[(size_t)k * AE_HIDDEN2 + j];
        }
        d_a2[k] = sum;
        d_z2[k] = d_a2[k] * relu_grad_from_activation(a2[k]);
    }

    for (int j = 0; j < AE_HIDDEN1; j++) {
        double sum = 0.0;
        for (int k = 0; k < AE_BOTTLENECK; k++) {
            sum += d_z2[k] * model->w2[(size_t)j * AE_BOTTLENECK + k];
        }
        d_a1[j] = sum;
        d_z1[j] = d_a1[j] * relu_grad_from_activation(a1[j]);
    }

    for (int j = 0; j < AE_HIDDEN2; j++) {
        for (int i = 0; i < n; i++) {
            model->w4[(size_t)j * n + i] -= learning_rate * a3[j] * d_out[i];
        }
    }
    for (int i = 0; i < n; i++) {
        model->b4[i] -= learning_rate * d_out[i];
    }

    for (int k = 0; k < AE_BOTTLENECK; k++) {
        for (int j = 0; j < AE_HIDDEN2; j++) {
            model->w3[(size_t)k * AE_HIDDEN2 + j] -= learning_rate * a2[k] * d_z3[j];
        }
    }
    for (int j = 0; j < AE_HIDDEN2; j++) {
        model->b3[j] -= learning_rate * d_z3[j];
    }

    for (int j = 0; j < AE_HIDDEN1; j++) {
        for (int k = 0; k < AE_BOTTLENECK; k++) {
            model->w2[(size_t)j * AE_BOTTLENECK + k] -= learning_rate * a1[j] * d_z2[k];
        }
    }
    for (int k = 0; k < AE_BOTTLENECK; k++) {
        model->b2[k] -= learning_rate * d_z2[k];
    }

    for (int i = 0; i < n; i++) {
        for (int j = 0; j < AE_HIDDEN1; j++) {
            model->w1[(size_t)i * AE_HIDDEN1 + j] -= learning_rate * input[i] * d_z1[j];
        }
    }
    for (int j = 0; j < AE_HIDDEN1; j++) {
        model->b1[j] -= learning_rate * d_z1[j];
    }

    return mse;
}

int ae_train(Autoencoder *model, const double *features, int row_count, int epochs, double learning_rate, const char *log_path)
{
    FILE *log_fp = NULL;
    int n = model->input_dim;

    if (row_count <= 0 || epochs <= 0) {
        return 0;
    }

    log_fp = fopen(log_path, "w");
    if (log_fp == NULL) {
        fprintf(stderr, "Failed to open train log: %s\n", log_path);
        return 0;
    }

    fprintf(log_fp, "epoch,loss\n");
    for (int epoch = 1; epoch <= epochs; epoch++) {
        double total_loss = 0.0;
        for (int row = 0; row < row_count; row++) {
            const double *input = &features[(size_t)row * (size_t)n];
            total_loss += ae_train_one(model, input, learning_rate);
        }
        total_loss /= (double)row_count;
        fprintf(log_fp, "%d,%.10f\n", epoch, total_loss);
        printf("epoch=%d loss=%.10f\n", epoch, total_loss);
    }

    fclose(log_fp);
    return 1;
}

static int compare_double(const void *a, const void *b)
{
    double da = *(const double *)a;
    double db = *(const double *)b;
    if (da < db) {
        return -1;
    }
    if (da > db) {
        return 1;
    }
    return 0;
}

double ae_percentile_error(const Autoencoder *model, const double *features, int row_count, double percentile)
{
    double *errors;
    int index;
    int n = model->input_dim;
    double value;

    if (row_count <= 0) {
        return 0.0;
    }

    errors = (double *)malloc((size_t)row_count * sizeof(double));
    if (errors == NULL) {
        return 0.0;
    }

    for (int row = 0; row < row_count; row++) {
        const double *input = &features[(size_t)row * (size_t)n];
        errors[row] = ae_reconstruct_error(model, input);
    }

    qsort(errors, (size_t)row_count, sizeof(double), compare_double);
    if (percentile < 0.0) {
        percentile = 0.0;
    } else if (percentile > 1.0) {
        percentile = 1.0;
    }
    index = (int)((double)(row_count - 1) * percentile);
    value = errors[index];
    free(errors);
    return value;
}

int ae_save(const char *path, const Autoencoder *model)
{
    FILE *fp;
    ModelHeader header;
    int n = model->input_dim;

    fp = fopen(path, "wb");
    if (fp == NULL) {
        fprintf(stderr, "Failed to save model: %s\n", path);
        return 0;
    }

    memset(&header, 0, sizeof(header));
    memcpy(header.magic, "NAE1C", 5);
    header.version = 1;
    header.input_dim = n;
    header.hidden1 = AE_HIDDEN1;
    header.bottleneck = AE_BOTTLENECK;
    header.hidden2 = AE_HIDDEN2;
    header.threshold = model->threshold;

    if (fwrite(&header, sizeof(header), 1, fp) != 1 ||
        !write_array(fp, model->w1, (size_t)n * AE_HIDDEN1) ||
        !write_array(fp, model->b1, AE_HIDDEN1) ||
        !write_array(fp, model->w2, (size_t)AE_HIDDEN1 * AE_BOTTLENECK) ||
        !write_array(fp, model->b2, AE_BOTTLENECK) ||
        !write_array(fp, model->w3, (size_t)AE_BOTTLENECK * AE_HIDDEN2) ||
        !write_array(fp, model->b3, AE_HIDDEN2) ||
        !write_array(fp, model->w4, (size_t)AE_HIDDEN2 * n) ||
        !write_array(fp, model->b4, (size_t)n)) {
        fprintf(stderr, "Failed to write model values: %s\n", path);
        fclose(fp);
        return 0;
    }

    fclose(fp);
    return 1;
}

int ae_load(const char *path, Autoencoder *model)
{
    FILE *fp;
    ModelHeader header;
    int n;

    fp = fopen(path, "rb");
    if (fp == NULL) {
        fprintf(stderr, "Failed to load model: %s\n", path);
        return 0;
    }

    if (fread(&header, sizeof(header), 1, fp) != 1 ||
        memcmp(header.magic, "NAE1C", 5) != 0 ||
        header.version != 1 ||
        header.input_dim <= 0 ||
        header.input_dim > MAX_FEATURES ||
        header.hidden1 != AE_HIDDEN1 ||
        header.bottleneck != AE_BOTTLENECK ||
        header.hidden2 != AE_HIDDEN2) {
        fprintf(stderr, "Invalid model file: %s\n", path);
        fclose(fp);
        return 0;
    }

    if (!ae_init(model, header.input_dim, 1)) {
        fclose(fp);
        return 0;
    }

    n = model->input_dim;
    model->threshold = header.threshold;

    if (!read_array(fp, model->w1, (size_t)n * AE_HIDDEN1) ||
        !read_array(fp, model->b1, AE_HIDDEN1) ||
        !read_array(fp, model->w2, (size_t)AE_HIDDEN1 * AE_BOTTLENECK) ||
        !read_array(fp, model->b2, AE_BOTTLENECK) ||
        !read_array(fp, model->w3, (size_t)AE_BOTTLENECK * AE_HIDDEN2) ||
        !read_array(fp, model->b3, AE_HIDDEN2) ||
        !read_array(fp, model->w4, (size_t)AE_HIDDEN2 * n) ||
        !read_array(fp, model->b4, (size_t)n)) {
        fprintf(stderr, "Failed to read model values: %s\n", path);
        ae_free(model);
        fclose(fp);
        return 0;
    }

    fclose(fp);
    return 1;
}
