#pragma once

struct ClipData
{
	WORD Format;
	HANDLE Data;

	ClipData() { Format = 0;  Data = NULL; }
	~ClipData() { if (Data) GlobalFree(Data); }
};

class CClipboard
{
public:
	static void Init(HWND hWnd);
	static void BackupClipboard();
	static CString GetClipboardText();
	static void RestoreClipboard();


private:
	static HWND m_hWnd;
	static ClipData* m_pLastClip;
};

