#include "detector.h"
#include "app_paths.h"
#include "gui.h"
#include "logger.h"

#include <stdio.h>
#include <string.h>

static void print_help(void)
{
    printf("Network Anomaly Detector (C Autoencoder)\n");
    printf("\n");
    printf("Usage:\n");
    printf("  network_anomaly_detector.exe train <train_csv>\n");
    printf("  network_anomaly_detector.exe test <test_csv>\n");
    printf("  network_anomaly_detector.exe detect <input_csv>\n");
    printf("  network_anomaly_detector.exe help\n");
    printf("\n");
    printf("Example:\n");
    printf("  network_anomaly_detector.exe train data/train_small.csv\n");
    printf("  network_anomaly_detector.exe test data/test_small.csv\n");
    printf("  network_anomaly_detector.exe detect data/realtime_sample.csv\n");
}

int main(int argc, char **argv)
{
    int exit_code = 1;

    app_paths_init();
    logger_init(argc > 0 ? argv[0] : "(unknown)");
    logger_info("project_root=%s", app_paths_get_root());
    logger_info("argc=%d", argc);
    for (int i = 0; i < argc; i++) {
        if (i == 0) {
            logger_info("argv[0]=(see program_path)");
        } else {
            logger_info("argv[%d]=%s", i, argv[i]);
        }
    }

    if (argc < 2 || strcmp(argv[1], "help") == 0 || strcmp(argv[1], "--help") == 0) {
        if (argc < 2) {
            logger_info("no command argument provided; starting gui");
            exit_code = run_gui();
            logger_info("exit_code=%d", exit_code);
            logger_close();
            return exit_code;
        }
        logger_info("help requested");
        print_help();
        exit_code = 0;
        logger_info("exit_code=%d", exit_code);
        logger_close();
        return exit_code;
    }

    if (strcmp(argv[1], "train") == 0) {
        if (argc != 3) {
            logger_error("invalid train arguments");
            print_help();
            logger_info("exit_code=1");
            logger_close();
            return 1;
        }
        logger_info("mode=train csv=%s", argv[2]);
        exit_code = run_train(argv[2]);
        logger_info("exit_code=%d", exit_code);
        logger_close();
        return exit_code;
    }

    if (strcmp(argv[1], "test") == 0) {
        if (argc != 3) {
            logger_error("invalid test arguments");
            print_help();
            logger_info("exit_code=1");
            logger_close();
            return 1;
        }
        logger_info("mode=test csv=%s", argv[2]);
        exit_code = run_test(argv[2]);
        logger_info("exit_code=%d", exit_code);
        logger_close();
        return exit_code;
    }

    if (strcmp(argv[1], "detect") == 0) {
        if (argc != 3) {
            logger_error("invalid detect arguments");
            print_help();
            logger_info("exit_code=1");
            logger_close();
            return 1;
        }
        logger_info("mode=detect csv=%s", argv[2]);
        exit_code = run_detect(argv[2]);
        logger_info("exit_code=%d", exit_code);
        logger_close();
        return exit_code;
    }

    logger_error("unknown command: %s", argv[1]);
    print_help();
    logger_info("exit_code=1");
    logger_close();
    return 1;
}
