#include "logger.h"

#include <errno.h>
#include <stdarg.h>
#include <stdio.h>
#include <string.h>
#include <time.h>

#ifdef _WIN32
#include <direct.h>
#include <windows.h>
#define GET_CWD(buffer, size) _getcwd(buffer, size)
#define MAKE_DIR(path) _mkdir(path)
#else
#include <sys/stat.h>
#include <unistd.h>
#define GET_CWD(buffer, size) getcwd(buffer, size)
#define MAKE_DIR(path) mkdir(path, 0755)
#endif

#define LOG_PATH "output/app.log"

static FILE *g_log_file = NULL;
static char g_log_path[256] = LOG_PATH;

#ifdef _WIN32
static int wide_to_utf8(const wchar_t *wide, char *out, int out_size)
{
    int converted;

    if (wide == NULL || out == NULL || out_size <= 0) {
        return 0;
    }

    converted = WideCharToMultiByte(CP_UTF8, 0, wide, -1, out, out_size, NULL, NULL);
    if (converted <= 0) {
        out[0] = '\0';
        return 0;
    }

    return 1;
}
#endif

static void ensure_output_dir(void)
{
    if (MAKE_DIR("output") != 0 && errno != EEXIST) {
        /* Fallback to app.log in the current directory if output/ cannot be created. */
        strcpy(g_log_path, "app.log");
    }
}

static void write_timestamp(FILE *fp)
{
    time_t now = time(NULL);
    struct tm *tm_info = localtime(&now);

    if (tm_info == NULL) {
        fprintf(fp, "0000-00-00 00:00:00");
        return;
    }

    fprintf(fp,
            "%04d-%02d-%02d %02d:%02d:%02d",
            tm_info->tm_year + 1900,
            tm_info->tm_mon + 1,
            tm_info->tm_mday,
            tm_info->tm_hour,
            tm_info->tm_min,
            tm_info->tm_sec);
}

static void logger_write(const char *level, const char *format, va_list args)
{
    if (g_log_file == NULL) {
        return;
    }

    write_timestamp(g_log_file);
    fprintf(g_log_file, " [%s] ", level);
    vfprintf(g_log_file, format, args);
    fprintf(g_log_file, "\n");
    fflush(g_log_file);
}

void logger_init(const char *program_path)
{
#ifdef _WIN32
    wchar_t wide_path[1024];
    wchar_t wide_cwd[1024];
    char utf8_text[4096];
#else
    char cwd[1024];
#endif

    ensure_output_dir();
    g_log_file = fopen(g_log_path, "a");
    if (g_log_file == NULL && strcmp(g_log_path, "app.log") != 0) {
        strcpy(g_log_path, "app.log");
        g_log_file = fopen(g_log_path, "a");
    }

    if (g_log_file == NULL) {
        return;
    }

    fprintf(g_log_file, "\n");
    logger_info("============================================================");
    logger_info("program started");

#ifdef _WIN32
    if (GetModuleFileNameW(NULL, wide_path, 1024) > 0 && wide_to_utf8(wide_path, utf8_text, sizeof(utf8_text))) {
        logger_info("program_path=%s", utf8_text);
    } else {
        logger_info("program_path=%s", program_path != NULL ? program_path : "(null)");
    }

    if (GetCurrentDirectoryW(1024, wide_cwd) > 0 && wide_to_utf8(wide_cwd, utf8_text, sizeof(utf8_text))) {
        logger_info("working_directory=%s", utf8_text);
    } else {
        logger_error("failed to read working directory");
    }
#else
    logger_info("program_path=%s", program_path != NULL ? program_path : "(null)");
    if (GET_CWD(cwd, sizeof(cwd)) != NULL) {
        logger_info("working_directory=%s", cwd);
    } else {
        logger_error("failed to read working directory");
    }
#endif
}

void logger_close(void)
{
    if (g_log_file == NULL) {
        return;
    }

    logger_info("program stopped");
    fclose(g_log_file);
    g_log_file = NULL;
}

void logger_info(const char *format, ...)
{
    va_list args;
    va_start(args, format);
    logger_write("INFO", format, args);
    va_end(args);
}

void logger_error(const char *format, ...)
{
    va_list args;
    va_start(args, format);
    logger_write("ERROR", format, args);
    va_end(args);
}

const char *logger_get_path(void)
{
    return g_log_path;
}
