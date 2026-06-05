#ifndef AUTOENCODER_H
#define AUTOENCODER_H

#include "csv_loader.h"

#define AE_HIDDEN1 32
#define AE_BOTTLENECK 8
#define AE_HIDDEN2 32

typedef struct {
    int input_dim;
    double threshold;
    double *w1;
    double *b1;
    double *w2;
    double *b2;
    double *w3;
    double *b3;
    double *w4;
    double *b4;
} Autoencoder;

int ae_init(Autoencoder *model, int input_dim, unsigned int seed);
void ae_free(Autoencoder *model);
int ae_train(Autoencoder *model, const double *features, int row_count, int epochs, double learning_rate, const char *log_path);
double ae_reconstruct_error(const Autoencoder *model, const double *input);
double ae_percentile_error(const Autoencoder *model, const double *features, int row_count, double percentile);
int ae_save(const char *path, const Autoencoder *model);
int ae_load(const char *path, Autoencoder *model);

#endif
