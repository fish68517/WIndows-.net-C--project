#include "gui.h"

#ifdef _WIN32

#include "detector.h"
#include "logger.h"

#include <math.h>
#include <stdio.h>
#include <string.h>
#include <windows.h>
#include <shellapi.h>
#include <shlobj.h>

#define APP_CLASS_NAME L"NetworkAnomalyDetectorWindow"
#define ID_BTN_SELECT_DIR 1001
#define ID_BTN_TRAIN 1002
#define ID_BTN_TEST 1003
#define ID_BTN_DETECT 1004
#define ID_BTN_LOG 1005
#define ID_BTN_OUTPUT 1006
#define ID_BTN_EXIT 1007
#define MAX_PATH_TEXT 1024
#define MAX_CHART_POINTS 5000

static HWND g_log_edit = NULL;
static HWND g_data_label = NULL;
static HFONT g_font = NULL;
static char g_data_dir[MAX_PATH_TEXT] = "data/processed/UNSW-NB15-demo";
static wchar_t g_data_dir_w[MAX_PATH_TEXT] = L"data\\processed\\UNSW-NB15-demo";
static double g_chart_errors[MAX_CHART_POINTS];
static int g_chart_predicted[MAX_CHART_POINTS];
static int g_chart_count = 0;
static double g_chart_threshold = 0.0;
static RECT g_chart_rect = {20, 425, 945, 645};

static void append_text(const wchar_t *text)
{
    int length;

    if (g_log_edit == NULL || text == NULL) {
        return;
    }

    length = GetWindowTextLengthW(g_log_edit);
    SendMessageW(g_log_edit, EM_SETSEL, (WPARAM)length, (LPARAM)length);
    SendMessageW(g_log_edit, EM_REPLACESEL, FALSE, (LPARAM)text);
    SendMessageW(g_log_edit, EM_SCROLLCARET, 0, 0);
}

static void append_line(const wchar_t *text)
{
    append_text(text);
    append_text(L"\r\n");
}

static void append_file_preview(const char *path, int max_lines)
{
    FILE *fp = fopen(path, "r");
    char line[1024];
    int count = 0;

    if (fp == NULL) {
        append_line(L"无法读取输出文件。");
        return;
    }

    while (fgets(line, sizeof(line), fp) != NULL && count < max_lines) {
        wchar_t wide_line[1200];
        int converted = MultiByteToWideChar(CP_UTF8, 0, line, -1, wide_line, 1200);
        if (converted == 0) {
            converted = MultiByteToWideChar(CP_ACP, 0, line, -1, wide_line, 1200);
        }
        if (converted > 0) {
            append_text(wide_line);
            if (wcschr(wide_line, L'\n') == NULL) {
                append_text(L"\r\n");
            }
        }
        count++;
    }

    fclose(fp);
}

static void replace_backslash_with_slash(char *text)
{
    for (char *p = text; *p != '\0'; p++) {
        if (*p == '\\') {
            *p = '/';
        }
    }
}

static void update_data_label(void)
{
    wchar_t text[MAX_PATH_TEXT + 64];

    if (g_data_label == NULL) {
        return;
    }

    swprintf(text, MAX_PATH_TEXT + 64, L"当前数据目录：%ls", g_data_dir_w);
    SetWindowTextW(g_data_label, text);
}

static int file_exists(const char *path)
{
    DWORD attrs;
    wchar_t wide[MAX_PATH_TEXT];

    if (MultiByteToWideChar(CP_UTF8, 0, path, -1, wide, MAX_PATH_TEXT) == 0) {
        return 0;
    }

    attrs = GetFileAttributesW(wide);
    return attrs != INVALID_FILE_ATTRIBUTES && (attrs & FILE_ATTRIBUTE_DIRECTORY) == 0;
}

static void build_data_path(char *out, size_t out_size, const char *file_name)
{
    size_t dir_len = strlen(g_data_dir);
    size_t file_len = strlen(file_name);

    if (out_size == 0) {
        return;
    }

    if (dir_len + 1 + file_len + 1 > out_size) {
        out[0] = '\0';
        logger_error("dataset path is too long");
        return;
    }

    strcpy(out, g_data_dir);
    out[dir_len] = '/';
    strcpy(out + dir_len + 1, file_name);
    replace_backslash_with_slash(out);
}

static int find_dataset_file(char *out, size_t out_size, const char **candidates, int count)
{
    for (int i = 0; i < count; i++) {
        build_data_path(out, out_size, candidates[i]);
        if (file_exists(out)) {
            return 1;
        }
    }
    out[0] = '\0';
    return 0;
}

static void set_status_start(const wchar_t *title)
{
    SetWindowTextW(g_log_edit, L"");
    append_line(title);
    append_line(L"请稍候，程序正在执行...");
    append_line(L"");
    UpdateWindow(g_log_edit);
    logger_info("gui action started data_dir=%s", g_data_dir);
}

static void reset_chart(void)
{
    g_chart_count = 0;
    g_chart_threshold = 0.0;
}

static void load_chart_from_result(void)
{
    FILE *fp = fopen("output/detect_result.csv", "r");
    char line[1024];

    reset_chart();

    if (fp == NULL) {
        logger_error("failed to load chart result file");
        return;
    }

    if (fgets(line, sizeof(line), fp) == NULL) {
        fclose(fp);
        return;
    }

    while (fgets(line, sizeof(line), fp) != NULL && g_chart_count < MAX_CHART_POINTS) {
        int record = 0;
        int predicted = 0;
        int label = -1;
        double error = 0.0;
        double threshold = 0.0;
        char status[64];

        if (sscanf(line, "%d,%lf,%lf,%d,%d,%63s",
                   &record, &error, &threshold, &predicted, &label, status) >= 4) {
            g_chart_errors[g_chart_count] = error;
            g_chart_predicted[g_chart_count] = predicted;
            g_chart_threshold = threshold;
            g_chart_count++;
        }
    }

    fclose(fp);
    logger_info("chart loaded points=%d threshold=%.10f", g_chart_count, g_chart_threshold);
}

static void draw_chart(HWND hwnd, HDC hdc)
{
    HPEN axis_pen = CreatePen(PS_SOLID, 1, RGB(80, 80, 80));
    HPEN error_pen = CreatePen(PS_SOLID, 2, RGB(33, 116, 191));
    HPEN threshold_pen = CreatePen(PS_DASH, 1, RGB(210, 60, 60));
    HBRUSH anomaly_brush = CreateSolidBrush(RGB(220, 40, 40));
    HBRUSH normal_brush = CreateSolidBrush(RGB(33, 116, 191));
    HBRUSH old_brush;
    HPEN old_pen;
    int left = g_chart_rect.left + 55;
    int top = g_chart_rect.top + 35;
    int right = g_chart_rect.right - 20;
    int bottom = g_chart_rect.bottom - 35;
    int width = right - left;
    int height = bottom - top;
    double max_value = g_chart_threshold;
    wchar_t label[256];

    FillRect(hdc, &g_chart_rect, (HBRUSH)(COLOR_WINDOW + 1));
    Rectangle(hdc, g_chart_rect.left, g_chart_rect.top, g_chart_rect.right, g_chart_rect.bottom);

    {
        const wchar_t *title = L"实时检测曲线：重构误差(蓝线) / 阈值(红虚线) / 异常报警(红点)";
        TextOutW(hdc, g_chart_rect.left + 12, g_chart_rect.top + 8, title, (int)wcslen(title));
    }

    if (g_chart_count <= 0) {
        const wchar_t *empty_text = L"点击“实时检测”后显示曲线图";
        TextOutW(hdc, left + 220, top + 65, empty_text, (int)wcslen(empty_text));
        return;
    }

    for (int i = 0; i < g_chart_count; i++) {
        if (g_chart_errors[i] > max_value) {
            max_value = g_chart_errors[i];
        }
    }
    if (max_value <= 0.0) {
        max_value = 1.0;
    }
    max_value *= 1.15;

    old_pen = (HPEN)SelectObject(hdc, axis_pen);
    MoveToEx(hdc, left, top, NULL);
    LineTo(hdc, left, bottom);
    LineTo(hdc, right, bottom);

    swprintf(label, 256, L"阈值 %.6f", g_chart_threshold);
    TextOutW(hdc, right - 130, top + 8, label, (int)wcslen(label));

    SelectObject(hdc, threshold_pen);
    {
        int y_threshold = bottom - (int)((g_chart_threshold / max_value) * height);
        if (y_threshold < top) {
            y_threshold = top;
        }
        if (y_threshold > bottom) {
            y_threshold = bottom;
        }
        MoveToEx(hdc, left, y_threshold, NULL);
        LineTo(hdc, right, y_threshold);
    }

    SelectObject(hdc, error_pen);
    for (int i = 0; i < g_chart_count; i++) {
        int x = left + (g_chart_count == 1 ? 0 : (int)((double)i * width / (double)(g_chart_count - 1)));
        int y = bottom - (int)((g_chart_errors[i] / max_value) * height);
        if (y < top) {
            y = top;
        }
        if (i == 0) {
            MoveToEx(hdc, x, y, NULL);
        } else {
            LineTo(hdc, x, y);
        }
    }

    old_brush = (HBRUSH)SelectObject(hdc, normal_brush);
    for (int i = 0; i < g_chart_count; i++) {
        int x = left + (g_chart_count == 1 ? 0 : (int)((double)i * width / (double)(g_chart_count - 1)));
        int y = bottom - (int)((g_chart_errors[i] / max_value) * height);
        if (y < top) {
            y = top;
        }
        SelectObject(hdc, g_chart_predicted[i] ? anomaly_brush : normal_brush);
        Ellipse(hdc, x - 3, y - 3, x + 4, y + 4);
    }

    swprintf(label, 256, L"记录数：%d", g_chart_count);
    TextOutW(hdc, left, bottom + 8, label, (int)wcslen(label));
    swprintf(label, 256, L"最大误差：%.6f", max_value / 1.15);
    TextOutW(hdc, left + 130, bottom + 8, label, (int)wcslen(label));

    SelectObject(hdc, old_brush);
    SelectObject(hdc, old_pen);
    DeleteObject(axis_pen);
    DeleteObject(error_pen);
    DeleteObject(threshold_pen);
    DeleteObject(anomaly_brush);
    DeleteObject(normal_brush);
    (void)hwnd;
}

static void append_train_explain(void)
{
    append_line(L"训练模型说明：");
    append_line(L"目的：只使用正常流量训练自编码器，让模型学习正常网络流量的特征规律。");
    append_line(L"功能：读取训练 CSV，归一化特征，训练神经网络，保存模型和阈值。");
    append_line(L"原理：自编码器尝试把输入流量重构出来；正常流量重构误差小，异常流量重构误差大。");
    append_line(L"");
}

static void append_test_explain(void)
{
    append_line(L"测试模型说明：");
    append_line(L"目的：用带标签的测试集验证模型检测效果。");
    append_line(L"功能：加载模型，计算每条流量的重构误差，输出正常/异常判断和评价指标。");
    append_line(L"原理：误差超过阈值判为异常，并与 label 对比得到 TP、TN、FP、FN、Accuracy、Precision、Recall、F1。");
    append_line(L"");
}

static void append_detect_explain(void)
{
    append_line(L"实时检测说明：");
    append_line(L"目的：模拟网络流量持续进入系统，实时分析并报警。");
    append_line(L"功能：逐条读取流量记录，计算重构误差，超过阈值时输出报警日志。");
    append_line(L"原理：图表横轴为流量记录序号，纵轴为重构误差；蓝线表示误差变化，红虚线表示异常阈值，红点表示报警。");
    append_line(L"");
}

static void run_train_action(HWND hwnd)
{
    const char *candidates[] = {"UNSW_NB15_training-set.csv", "train_small.csv"};
    char path[MAX_PATH_TEXT];
    int code;

    reset_chart();
    InvalidateRect(hwnd, &g_chart_rect, TRUE);
    set_status_start(L"开始训练模型");
    append_train_explain();

    if (!find_dataset_file(path, sizeof(path), candidates, 2)) {
        append_line(L"训练失败：当前数据目录中没有 UNSW_NB15_training-set.csv 或 train_small.csv。");
        MessageBoxW(hwnd, L"训练数据文件不存在，请先选择正确的数据目录。", L"训练模型", MB_ICONERROR);
        return;
    }

    append_line(L"训练文件：");
    {
        wchar_t wide_path[MAX_PATH_TEXT];
        MultiByteToWideChar(CP_UTF8, 0, path, -1, wide_path, MAX_PATH_TEXT);
        append_line(wide_path);
    }

    code = run_train(path);
    append_line(code == 0 ? L"训练完成。" : L"训练失败，请查看 output/app.log。");
    append_line(L"训练日志：output/train_log.txt");
    append_line(L"模型文件：model/autoencoder_model.bin");
    append_line(L"归一化参数：model/normalize_params.txt");
    logger_info("gui train action exit_code=%d path=%s", code, path);
    MessageBoxW(hwnd, code == 0 ? L"训练完成" : L"训练失败，请查看日志", L"训练模型", code == 0 ? MB_ICONINFORMATION : MB_ICONERROR);
}

static void run_test_action(HWND hwnd)
{
    const char *candidates[] = {"UNSW_NB15_testing-set.csv", "test_small.csv"};
    char path[MAX_PATH_TEXT];
    int code;

    reset_chart();
    InvalidateRect(hwnd, &g_chart_rect, TRUE);
    set_status_start(L"开始测试模型");
    append_test_explain();

    if (!find_dataset_file(path, sizeof(path), candidates, 2)) {
        append_line(L"测试失败：当前数据目录中没有 UNSW_NB15_testing-set.csv 或 test_small.csv。");
        MessageBoxW(hwnd, L"测试数据文件不存在，请先选择正确的数据目录。", L"测试模型", MB_ICONERROR);
        return;
    }

    code = run_test(path);
    append_line(code == 0 ? L"测试完成。" : L"测试失败，请查看 output/app.log。");
    append_line(L"");
    append_line(L"测试结果预览：");
    append_file_preview("output/detect_result.csv", 16);
    append_line(L"");
    append_line(L"评价指标：");
    append_file_preview("output/test_metrics.txt", 16);
    logger_info("gui test action exit_code=%d path=%s", code, path);
    MessageBoxW(hwnd, code == 0 ? L"测试完成" : L"测试失败，请查看日志", L"测试模型", code == 0 ? MB_ICONINFORMATION : MB_ICONERROR);
}

static void run_detect_action(HWND hwnd)
{
    const char *candidates[] = {"realtime_sample.csv", "UNSW_NB15_testing-set.csv", "test_small.csv"};
    char path[MAX_PATH_TEXT];
    int code;

    set_status_start(L"开始实时检测");
    append_detect_explain();

    if (!find_dataset_file(path, sizeof(path), candidates, 3)) {
        append_line(L"检测失败：当前数据目录中没有 realtime_sample.csv、UNSW_NB15_testing-set.csv 或 test_small.csv。");
        MessageBoxW(hwnd, L"检测数据文件不存在，请先选择正确的数据目录。", L"实时检测", MB_ICONERROR);
        return;
    }

    code = run_detect(path);
    append_line(code == 0 ? L"检测完成。" : L"检测失败，请查看 output/app.log。");
    append_line(L"");
    append_line(L"检测结果预览：");
    append_file_preview("output/detect_result.csv", 14);
    append_line(L"");
    append_line(L"报警日志预览：");
    append_file_preview("output/alarm_log.txt", 14);
    load_chart_from_result();
    InvalidateRect(hwnd, &g_chart_rect, TRUE);
    logger_info("gui detect action exit_code=%d path=%s", code, path);
    MessageBoxW(hwnd, code == 0 ? L"检测完成，曲线图已刷新" : L"检测失败，请查看日志", L"实时检测", code == 0 ? MB_ICONINFORMATION : MB_ICONERROR);
}

static void select_data_directory(HWND hwnd)
{
    BROWSEINFOW browse;
    LPITEMIDLIST item;
    wchar_t selected[MAX_PATH_TEXT];

    ZeroMemory(&browse, sizeof(browse));
    browse.hwndOwner = hwnd;
    browse.lpszTitle = L"请选择包含 UNSW_NB15_training-set.csv / UNSW_NB15_testing-set.csv 的数据目录";
    browse.ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE;

    item = SHBrowseForFolderW(&browse);
    if (item == NULL) {
        return;
    }

    if (SHGetPathFromIDListW(item, selected)) {
        wchar_t cwd[MAX_PATH_TEXT];
        const wchar_t *display_path = selected;
        wchar_t relative[MAX_PATH_TEXT];
        DWORD cwd_len = GetCurrentDirectoryW(MAX_PATH_TEXT, cwd);

        if (cwd_len > 0 &&
            _wcsnicmp(selected, cwd, cwd_len) == 0 &&
            (selected[cwd_len] == L'\\' || selected[cwd_len] == L'/')) {
            wcsncpy(relative, selected + cwd_len + 1, MAX_PATH_TEXT - 1);
            relative[MAX_PATH_TEXT - 1] = L'\0';
            display_path = relative;
        }

        wcsncpy(g_data_dir_w, display_path, MAX_PATH_TEXT - 1);
        g_data_dir_w[MAX_PATH_TEXT - 1] = L'\0';
        WideCharToMultiByte(CP_UTF8, 0, g_data_dir_w, -1, g_data_dir, MAX_PATH_TEXT, NULL, NULL);
        replace_backslash_with_slash(g_data_dir);
        update_data_label();

        SetWindowTextW(g_log_edit, L"");
        append_line(L"已加载数据目录：");
        append_line(g_data_dir_w);
        append_line(L"");
        append_line(L"目录中应包含：UNSW_NB15_training-set.csv、UNSW_NB15_testing-set.csv。");
        append_line(L"如果存在 realtime_sample.csv，实时检测会优先使用该文件。");
        logger_info("data directory selected: %s", g_data_dir);
    }

    CoTaskMemFree(item);
}

static HWND create_button(HWND parent, const wchar_t *text, int id, int x, int y, int w, int h)
{
    HWND button = CreateWindowW(
        L"BUTTON",
        text,
        WS_CHILD | WS_VISIBLE | BS_PUSHBUTTON,
        x,
        y,
        w,
        h,
        parent,
        (HMENU)(INT_PTR)id,
        GetModuleHandleW(NULL),
        NULL);
    if (button != NULL && g_font != NULL) {
        SendMessageW(button, WM_SETFONT, (WPARAM)g_font, TRUE);
    }
    return button;
}

static LRESULT CALLBACK window_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
{
    switch (msg) {
    case WM_CREATE: {
        HWND title;

        g_font = CreateFontW(
            18,
            0,
            0,
            0,
            FW_NORMAL,
            FALSE,
            FALSE,
            FALSE,
            DEFAULT_CHARSET,
            OUT_DEFAULT_PRECIS,
            CLIP_DEFAULT_PRECIS,
            CLEARTYPE_QUALITY,
            DEFAULT_PITCH | FF_SWISS,
            L"Microsoft YaHei UI");

        title = CreateWindowW(
            L"STATIC",
            L"基于深度学习的网络异常流量检测系统",
            WS_CHILD | WS_VISIBLE,
            20,
            16,
            920,
            30,
            hwnd,
            NULL,
            GetModuleHandleW(NULL),
            NULL);
        if (title != NULL && g_font != NULL) {
            SendMessageW(title, WM_SETFONT, (WPARAM)g_font, TRUE);
        }

        g_data_label = CreateWindowW(
            L"STATIC",
            L"",
            WS_CHILD | WS_VISIBLE,
            20,
            52,
            920,
            26,
            hwnd,
            NULL,
            GetModuleHandleW(NULL),
            NULL);
        if (g_data_label != NULL && g_font != NULL) {
            SendMessageW(g_data_label, WM_SETFONT, (WPARAM)g_font, TRUE);
        }
        update_data_label();

        create_button(hwnd, L"选择数据目录", ID_BTN_SELECT_DIR, 20, 86, 130, 34);
        create_button(hwnd, L"训练模型", ID_BTN_TRAIN, 160, 86, 110, 34);
        create_button(hwnd, L"测试模型", ID_BTN_TEST, 280, 86, 110, 34);
        create_button(hwnd, L"实时检测", ID_BTN_DETECT, 400, 86, 110, 34);
        create_button(hwnd, L"查看日志", ID_BTN_LOG, 520, 86, 110, 34);
        create_button(hwnd, L"输出目录", ID_BTN_OUTPUT, 640, 86, 110, 34);
        create_button(hwnd, L"退出", ID_BTN_EXIT, 760, 86, 90, 34);

        g_log_edit = CreateWindowExW(
            WS_EX_CLIENTEDGE,
            L"EDIT",
            L"",
            WS_CHILD | WS_VISIBLE | WS_VSCROLL | ES_MULTILINE | ES_AUTOVSCROLL | ES_READONLY,
            20,
            135,
            925,
            270,
            hwnd,
            NULL,
            GetModuleHandleW(NULL),
            NULL);
        if (g_log_edit != NULL && g_font != NULL) {
            SendMessageW(g_log_edit, WM_SETFONT, (WPARAM)g_font, TRUE);
        }

        append_line(L"程序已启动。默认加载公开数据集演示目录：data\\processed\\UNSW-NB15-demo。");
        append_line(L"如需使用全量公开数据集，请点击“选择数据目录”，选择 data\\processed\\UNSW-NB15。");
        append_line(L"推荐演示顺序：训练模型 -> 测试模型 -> 实时检测。");
        append_line(L"实时检测完成后，下方会显示误差曲线、阈值线和异常报警点。");
        logger_info("gui window created");
        return 0;
    }

    case WM_PAINT: {
        PAINTSTRUCT ps;
        HDC hdc = BeginPaint(hwnd, &ps);
        if (g_font != NULL) {
            SelectObject(hdc, g_font);
        }
        draw_chart(hwnd, hdc);
        EndPaint(hwnd, &ps);
        return 0;
    }

    case WM_COMMAND:
        switch (LOWORD(wparam)) {
        case ID_BTN_SELECT_DIR:
            select_data_directory(hwnd);
            return 0;
        case ID_BTN_TRAIN:
            run_train_action(hwnd);
            return 0;
        case ID_BTN_TEST:
            run_test_action(hwnd);
            return 0;
        case ID_BTN_DETECT:
            run_detect_action(hwnd);
            return 0;
        case ID_BTN_LOG:
            ShellExecuteW(hwnd, L"open", L"output\\app.log", NULL, NULL, SW_SHOWNORMAL);
            return 0;
        case ID_BTN_OUTPUT:
            ShellExecuteW(hwnd, L"open", L"output", NULL, NULL, SW_SHOWNORMAL);
            return 0;
        case ID_BTN_EXIT:
            DestroyWindow(hwnd);
            return 0;
        default:
            break;
        }
        break;

    case WM_DESTROY:
        logger_info("gui window destroyed");
        if (g_font != NULL) {
            DeleteObject(g_font);
            g_font = NULL;
        }
        PostQuitMessage(0);
        return 0;

    default:
        break;
    }

    return DefWindowProcW(hwnd, msg, wparam, lparam);
}

int run_gui(void)
{
    HINSTANCE instance = GetModuleHandleW(NULL);
    WNDCLASSW wc;
    HWND hwnd;
    MSG msg;

    CoInitialize(NULL);

    ZeroMemory(&wc, sizeof(wc));
    wc.lpfnWndProc = window_proc;
    wc.hInstance = instance;
    wc.lpszClassName = APP_CLASS_NAME;
    wc.hCursor = LoadCursor(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);

    if (!RegisterClassW(&wc)) {
        MessageBoxW(NULL, L"注册窗口类失败", L"启动失败", MB_ICONERROR);
        logger_error("failed to register gui window class");
        CoUninitialize();
        return 1;
    }

    hwnd = CreateWindowExW(
        0,
        APP_CLASS_NAME,
        L"网络异常流量检测系统",
        WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        990,
        720,
        NULL,
        NULL,
        instance,
        NULL);

    if (hwnd == NULL) {
        MessageBoxW(NULL, L"创建窗口失败", L"启动失败", MB_ICONERROR);
        logger_error("failed to create gui window");
        CoUninitialize();
        return 1;
    }

    ShowWindow(hwnd, SW_SHOW);
    UpdateWindow(hwnd);
    logger_info("gui message loop started");

    while (GetMessageW(&msg, NULL, 0, 0) > 0) {
        TranslateMessage(&msg);
        DispatchMessageW(&msg);
    }

    logger_info("gui message loop stopped");
    CoUninitialize();
    return (int)msg.wParam;
}

#else

int run_gui(void)
{
    return 1;
}

#endif
