#ifndef PREPROCESS_H
#define PREPROCESS_H

#include "csv_loader.h"

typedef struct {
    int feature_count;
    double min_values[MAX_FEATURES];
    double max_values[MAX_FEATURES];
} Normalizer;

void normalizer_fit(const Dataset *dataset, Normalizer *normalizer);
void normalizer_apply(Dataset *dataset, const Normalizer *normalizer);
int normalizer_save(const char *path, const Normalizer *normalizer);
int normalizer_load(const char *path, Normalizer *normalizer);

#endif
