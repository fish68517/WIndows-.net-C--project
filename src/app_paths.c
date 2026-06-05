#include "app_paths.h"

#include <stdio.h>
#include <string.h>

#ifdef _WIN32
#include <direct.h>
#include <windows.h>
#else
#include <unistd.h>
#endif

static char g_root_path[1024] = ".";

#ifdef _WIN32
static void dirname_w(wchar_t *path)
{
    wchar_t *last_slash = wcsrchr(path, L'\\');
    wchar_t *last_forward = wcsrchr(path, L'/');
    wchar_t *last = last_slash;

    if (last_forward != NULL && (last == NULL || last_forward > last)) {
        last = last_forward;
    }
    if (last != NULL) {
        *last = L'\0';
    }
}

static const wchar_t *basename_w(const wchar_t *path)
{
    const wchar_t *last_slash = wcsrchr(path, L'\\');
    const wchar_t *last_forward = wcsrchr(path, L'/');
    const wchar_t *last = last_slash;

    if (last_forward != NULL && (last == NULL || last_forward > last)) {
        last = last_forward;
    }
    return last != NULL ? last + 1 : path;
}
#endif

int app_paths_init(void)
{
#ifdef _WIN32
    wchar_t exe_path[MAX_PATH];
    wchar_t exe_dir[MAX_PATH];
    wchar_t root_dir[MAX_PATH];
    char narrow_root[sizeof(g_root_path)];

    if (GetModuleFileNameW(NULL, exe_path, MAX_PATH) == 0) {
        return 0;
    }

    wcsncpy(exe_dir, exe_path, MAX_PATH - 1);
    exe_dir[MAX_PATH - 1] = L'\0';
    dirname_w(exe_dir);

    wcsncpy(root_dir, exe_dir, MAX_PATH - 1);
    root_dir[MAX_PATH - 1] = L'\0';

    if (_wcsicmp(basename_w(root_dir), L"build") == 0) {
        dirname_w(root_dir);
    }

    if (!SetCurrentDirectoryW(root_dir)) {
        return 0;
    }

    if (WideCharToMultiByte(CP_UTF8, 0, root_dir, -1, narrow_root, sizeof(narrow_root), NULL, NULL) > 0) {
        strncpy(g_root_path, narrow_root, sizeof(g_root_path) - 1);
        g_root_path[sizeof(g_root_path) - 1] = '\0';
    }
    return 1;
#else
    char cwd[sizeof(g_root_path)];
    if (getcwd(cwd, sizeof(cwd)) != NULL) {
        strncpy(g_root_path, cwd, sizeof(g_root_path) - 1);
        g_root_path[sizeof(g_root_path) - 1] = '\0';
    }
    return 1;
#endif
}

const char *app_paths_get_root(void)
{
    return g_root_path;
}
