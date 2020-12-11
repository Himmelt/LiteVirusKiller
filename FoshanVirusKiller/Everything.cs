using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace FoshanVirusKiller
{
    public partial class MainWindow : Window
    {
        const int EVERYTHING_OK = 0; // no error detected
        const int EVERYTHING_ERROR_MEMORY = 1; // out of memory.
        const int EVERYTHING_ERROR_IPC = 2; // Everything search client is not running
        const int EVERYTHING_ERROR_REGISTERCLASSEX = 3; // unable to register window class.
        const int EVERYTHING_ERROR_CREATEWINDOW = 4; // unable to create listening window
        const int EVERYTHING_ERROR_CREATETHREAD = 5; // unable to create listening thread
        const int EVERYTHING_ERROR_INVALIDINDEX = 6; // invalid index
        const int EVERYTHING_ERROR_INVALIDCALL = 7; // invalid call
        const int EVERYTHING_ERROR_INVALIDREQUEST = 8; // invalid request data, request data first.
        const int EVERYTHING_ERROR_INVALIDPARAMETER = 9; // bad parameter.

        const int EVERYTHING_REQUEST_FILE_NAME = 0x00000001;
        const int EVERYTHING_REQUEST_PATH = 0x00000002;
        const int EVERYTHING_REQUEST_FULL_PATH_AND_FILE_NAME = 0x00000004;
        const int EVERYTHING_REQUEST_EXTENSION = 0x00000008;
        const int EVERYTHING_REQUEST_SIZE = 0x00000010;
        const int EVERYTHING_REQUEST_DATE_CREATED = 0x00000020;
        const int EVERYTHING_REQUEST_DATE_MODIFIED = 0x00000040;
        const int EVERYTHING_REQUEST_DATE_ACCESSED = 0x00000080;
        const int EVERYTHING_REQUEST_ATTRIBUTES = 0x00000100;
        const int EVERYTHING_REQUEST_FILE_LIST_FILE_NAME = 0x00000200;
        const int EVERYTHING_REQUEST_RUN_COUNT = 0x00000400;
        const int EVERYTHING_REQUEST_DATE_RUN = 0x00000800;
        const int EVERYTHING_REQUEST_DATE_RECENTLY_CHANGED = 0x00001000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_FILE_NAME = 0x00002000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_PATH = 0x00004000;
        const int EVERYTHING_REQUEST_HIGHLIGHTED_FULL_PATH_AND_FILE_NAME = 0x00008000;

        const int EVERYTHING_SORT_NAME_ASCENDING = 1;
        const int EVERYTHING_SORT_NAME_DESCENDING = 2;
        const int EVERYTHING_SORT_PATH_ASCENDING = 3;
        const int EVERYTHING_SORT_PATH_DESCENDING = 4;
        const int EVERYTHING_SORT_SIZE_ASCENDING = 5;
        const int EVERYTHING_SORT_SIZE_DESCENDING = 6;
        const int EVERYTHING_SORT_EXTENSION_ASCENDING = 7;
        const int EVERYTHING_SORT_EXTENSION_DESCENDING = 8;
        const int EVERYTHING_SORT_TYPE_NAME_ASCENDING = 9;
        const int EVERYTHING_SORT_TYPE_NAME_DESCENDING = 10;
        const int EVERYTHING_SORT_DATE_CREATED_ASCENDING = 11;
        const int EVERYTHING_SORT_DATE_CREATED_DESCENDING = 12;
        const int EVERYTHING_SORT_DATE_MODIFIED_ASCENDING = 13;
        const int EVERYTHING_SORT_DATE_MODIFIED_DESCENDING = 14;
        const int EVERYTHING_SORT_ATTRIBUTES_ASCENDING = 15;
        const int EVERYTHING_SORT_ATTRIBUTES_DESCENDING = 16;
        const int EVERYTHING_SORT_FILE_LIST_FILENAME_ASCENDING = 17;
        const int EVERYTHING_SORT_FILE_LIST_FILENAME_DESCENDING = 18;
        const int EVERYTHING_SORT_RUN_COUNT_ASCENDING = 19;
        const int EVERYTHING_SORT_RUN_COUNT_DESCENDING = 20;
        const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_ASCENDING = 21;
        const int EVERYTHING_SORT_DATE_RECENTLY_CHANGED_DESCENDING = 22;
        const int EVERYTHING_SORT_DATE_ACCESSED_ASCENDING = 23;
        const int EVERYTHING_SORT_DATE_ACCESSED_DESCENDING = 24;
        const int EVERYTHING_SORT_DATE_RUN_ASCENDING = 25;
        const int EVERYTHING_SORT_DATE_RUN_DESCENDING = 26;

        const int EVERYTHING_TARGET_MACHINE_X86 = 1;
        const int EVERYTHING_TARGET_MACHINE_X64 = 2;
        const int EVERYTHING_TARGET_MACHINE_ARM = 3;

        const string dllPath = @"C:\ProgramData\FoshanVirusKiller\Everything64.dll";

        [DllImport(dllPath, CharSet = CharSet.Unicode)]
        public static extern UInt32 Everything_SetSearchW(string lpSearchString);
        [DllImport(dllPath)]
        public static extern void Everything_SetMatchPath(bool bEnable);
        [DllImport(dllPath)]
        public static extern void Everything_SetMatchCase(bool bEnable);
        [DllImport(dllPath)]
        public static extern void Everything_SetMatchWholeWord(bool bEnable);
        [DllImport(dllPath)]
        public static extern void Everything_SetRegex(bool bEnable);
        [DllImport(dllPath)]
        public static extern void Everything_SetMax(UInt32 dwMax);
        [DllImport(dllPath)]
        public static extern void Everything_SetOffset(UInt32 dwOffset);

        [DllImport(dllPath)]
        public static extern bool Everything_GetMatchPath();
        [DllImport(dllPath)]
        public static extern bool Everything_GetMatchCase();
        [DllImport(dllPath)]
        public static extern bool Everything_GetMatchWholeWord();
        [DllImport(dllPath)]
        public static extern bool Everything_GetRegex();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetMax();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetOffset();
        [DllImport(dllPath)]
        public static extern IntPtr Everything_GetSearchW();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetLastError();

        [DllImport(dllPath)]
        public static extern bool Everything_QueryW(bool bWait);

        [DllImport(dllPath)]
        public static extern void Everything_SortResultsByPath();

        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetNumFileResults();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetNumFolderResults();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetNumResults();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetTotFileResults();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetTotFolderResults();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetTotResults();
        [DllImport(dllPath)]
        public static extern bool Everything_IsVolumeResult(UInt32 nIndex);
        [DllImport(dllPath)]
        public static extern bool Everything_IsFolderResult(UInt32 nIndex);
        [DllImport(dllPath)]
        public static extern bool Everything_IsFileResult(UInt32 nIndex);
        [DllImport(dllPath)]
        public static extern bool Everything_IsDBLoaded();
        [DllImport(dllPath)]
        public static extern bool Everything_RebuildDB();
        [DllImport(dllPath, CharSet = CharSet.Unicode)]
        public static extern void Everything_GetResultFullPathName(UInt32 nIndex, StringBuilder lpString, UInt32 nMaxCount);
        [DllImport(dllPath)]
        public static extern void Everything_Reset();

        [DllImport(dllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileName(UInt32 nIndex);

        // Everything 1.4
        [DllImport(dllPath)]
        public static extern void Everything_SetSort(UInt32 dwSortType);
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetSort();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetResultListSort();
        [DllImport(dllPath)]
        public static extern void Everything_SetRequestFlags(UInt32 dwRequestFlags);
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetRequestFlags();
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetResultListRequestFlags();
        [DllImport(dllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultExtension(UInt32 nIndex);
        [DllImport(dllPath)]
        public static extern bool Everything_GetResultSize(UInt32 nIndex, out long lpFileSize);
        [DllImport(dllPath)]
        public static extern bool Everything_GetResultDateCreated(UInt32 nIndex, out long lpFileTime);
        [DllImport(dllPath)]
        public static extern bool Everything_GetResultDateModified(UInt32 nIndex, out long lpFileTime);
        [DllImport(dllPath)]
        public static extern bool Everything_GetResultDateAccessed(UInt32 nIndex, out long lpFileTime);
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetResultAttributes(UInt32 nIndex);
        [DllImport(dllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultFileListFileName(UInt32 nIndex);
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetResultRunCount(UInt32 nIndex);
        [DllImport(dllPath)]
        public static extern bool Everything_GetResultDateRun(UInt32 nIndex, out long lpFileTime);
        [DllImport(dllPath)]
        public static extern bool Everything_GetResultDateRecentlyChanged(UInt32 nIndex, out long lpFileTime);
        [DllImport(dllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFileName(UInt32 nIndex);
        [DllImport(dllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedPath(UInt32 nIndex);
        [DllImport(dllPath, CharSet = CharSet.Unicode)]
        public static extern IntPtr Everything_GetResultHighlightedFullPathAndFileName(UInt32 nIndex);
        [DllImport(dllPath)]
        public static extern UInt32 Everything_GetRunCountFromFileName(string lpFileName);
        [DllImport(dllPath)]
        public static extern bool Everything_SetRunCountFromFileName(string lpFileName, UInt32 dwRunCount);
        [DllImport(dllPath)]
        public static extern UInt32 Everything_IncRunCountFromFileName(string lpFileName);
    }
}
