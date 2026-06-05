#ifndef CSV_LOADER_H
#define CSV_LOADER_H

#define FEATURE_COUNT 16
#define MAX_FEATURES 32
#define MAX_CSV_FIELDS 128

typedef struct {
    double *features;
    int *labels;
    int row_count;
    int feature_count;
    int has_label;
} Dataset;

extern const char *FEATURE_NAMES[FEATURE_COUNT];

int csv_load_dataset(const char *path, int normal_only, Dataset *dataset);
void dataset_free(Dataset *dataset);

#endif
