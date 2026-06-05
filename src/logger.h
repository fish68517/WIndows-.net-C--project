#ifndef LOGGER_H
#define LOGGER_H

void logger_init(const char *program_path);
void logger_close(void);
void logger_info(const char *format, ...);
void logger_error(const char *format, ...);
const char *logger_get_path(void);

#endif
