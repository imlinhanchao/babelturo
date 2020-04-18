#include "StdAfx.h"
#include "Hook.h"
#include "resource.h"
#include "TuroDlg.h"

#define VK_C			0x43

CHook::CHook(void)
{
	m_bMove = false;
	ZeroMemory(&m_ptDown, sizeof(m_ptDown));
	ZeroMemory(&m_ptUp, sizeof(m_ptUp));
	m_callback = NULL;
	m_lpParam = NULL;
	m_bInstall = false;
}

CHook::~CHook()
{
	Uninstall();
}

void CHook::Install( HWND hWnd )
{
	m_Hook = SetWindowsHookEx(WH_MOUSE_LL, (HOOKPROC)&MouseProc, GetModuleHandle(NULL), NULL);
	m_Clip.Init(hWnd);
	m_bInstall = true;
}

void CHook::Uninstall( void )
{
	if(m_bInstall) UnhookWindowsHookEx(m_Hook);
	m_bInstall = false;
}

int CALLBACK CHook::MouseProc( int nCode, WPARAM wParam, LPARAM lParam )
{
	MSLLHOOKSTRUCT* p = (MSLLHOOKSTRUCT*)lParam;
	CHook* pThis = &((CTuroDlg*)AfxGetMainWnd())->m_Hook;
	switch(wParam)
	{
	case WM_LBUTTONDOWN:
		pThis->m_ptDown = p->pt;
		pThis->m_bMove = false;
		break;
	case WM_MOUSEMOVE:
		pThis->m_bMove = true;
		break;
	case WM_LBUTTONUP:
		pThis->m_ptUp = p->pt;
		AfxBeginThread(SelectTextThread, pThis);

		break;
	}
	return CallNextHookEx(pThis->m_Hook, nCode, wParam, lParam);
}

void CHook::SetSelectCallback( SELECTPROC callback, LPVOID lpParam )
{
	m_callback = callback;
	m_lpParam = lpParam;
}

void CHook::GetSelectedText()
{
	if(!m_bMove) return;
	HWND hPosWnd = ::WindowFromPoint(m_ptUp);
	if(!hPosWnd) {
		return;
	}

	m_Clip.BackupClipboard();

	::SendMessage(hPosWnd, WM_COPY,0, 0);

	CString sText = m_Clip.GetClipboardText();

	if (sText.IsEmpty())
	{
		PressKey(VK_C, false, false, true);
		Sleep(100);
		sText = m_Clip.GetClipboardText();

	}

	m_Clip.RestoreClipboard();

	if (m_callback != NULL && !sText.IsEmpty()) 
		m_callback(m_ptDown, m_ptUp, sText, m_lpParam);

	m_bMove = false;
}

bool CHook::PressKey( BYTE vk, bool bAlt, bool bShift, bool bCtrl )
{
	if(0 == vk)
	{
		return false;
	}

	if(bAlt)   { keybd_event(VK_MENU,    MapVirtualKey(VK_MENU,    0), 0, 0); }
	if(bShift) { keybd_event(VK_SHIFT,   MapVirtualKey(VK_SHIFT,   0), 0, 0); }
	if(bCtrl)  { keybd_event(VK_CONTROL, MapVirtualKey(VK_CONTROL, 0), 0, 0); }

	keybd_event(vk, MapVirtualKey(vk, 0), 0, 0);
	Sleep(100);
	keybd_event(vk, MapVirtualKey(vk, 0), KEYEVENTF_KEYUP, 0);

	if(bAlt)   { keybd_event(VK_MENU,    MapVirtualKey(VK_MENU,    0), KEYEVENTF_KEYUP, 0); }
	if(bShift) { keybd_event(VK_SHIFT,   MapVirtualKey(VK_SHIFT,   0), KEYEVENTF_KEYUP, 0); }
	if(bCtrl)  { keybd_event(VK_CONTROL, MapVirtualKey(VK_CONTROL, 0), KEYEVENTF_KEYUP, 0); }

	return true;
}

UINT CHook::SelectTextThread( LPVOID lpParam )
{
	CHook* pThis = (CHook*)lpParam;
	pThis->GetSelectedText();
	return 0;
}
