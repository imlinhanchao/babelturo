#include "stdafx.h"
#include "Clipboard.h"

HWND CClipboard::m_hWnd = NULL;
ClipData* CClipboard::m_pLastClip = NULL;

void CClipboard::Init( HWND hWnd )
{
	m_hWnd = hWnd;
}

void CClipboard::BackupClipboard()
{
	if (!::OpenClipboard(m_hWnd)) return; 

	m_pLastClip = new ClipData;
	m_pLastClip->Format = EnumClipboardFormats(0);
	HANDLE Data = GetClipboardData(m_pLastClip->Format);
	m_pLastClip->Data = GlobalAlloc(GMEM_MOVEABLE | GMEM_DDESHARE, GlobalSize(Data));
	LPVOID DataMem = GlobalLock(m_pLastClip->Data);
	if (DataMem != NULL) {
		memcpy(DataMem, GlobalLock(Data), GlobalSize(Data));
		GlobalUnlock((HANDLE)m_pLastClip->Data);
		GlobalUnlock((HANDLE)Data);
	}
	EmptyClipboard();
	CloseClipboard();
}

CString CClipboard::GetClipboardText()
{
	CString sText;
	if (!::OpenClipboard(m_hWnd)) return sText;

	if (!IsClipboardFormatAvailable(CF_TEXT)) return sText;

	char* szBuffer;  

	HANDLE hClip = GetClipboardData(CF_TEXT);  
	szBuffer = (char*)GlobalLock(hClip);
	sText = Cvt::ToString(szBuffer);

	GlobalUnlock(hClip);
	CloseClipboard();  

	return sText;
}

void CClipboard::RestoreClipboard()
{
	if (!::OpenClipboard(m_hWnd)) return;
	EmptyClipboard();

	if (m_pLastClip->Format != 0) 
		SetClipboardData(m_pLastClip->Format, m_pLastClip->Data);

	m_pLastClip->Data = NULL;
	delete m_pLastClip;

	CloseClipboard();
}
