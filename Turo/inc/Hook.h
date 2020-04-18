#pragma once

typedef void (CALLBACK* SELECTPROC)(POINT ptDown, POINT ptUp, CString sText, LPVOID lpParam);

class CHook
{
public:
	CHook();
	~CHook();

	void Install(HWND hWnd);
	void Uninstall(void);
	void SetSelectCallback(SELECTPROC callback, LPVOID lpParam);

private:
	static int CALLBACK MouseProc(int nCode, WPARAM wParam, LPARAM lParam);
	static UINT SelectTextThread(LPVOID lpParam);

	void GetSelectedText();
	bool PressKey( BYTE vk, bool bAlt, bool bShift, bool bCtrl );

	HHOOK m_Hook;
	bool m_bMove;
	bool m_bInstall;

	POINT m_ptDown;
	POINT m_ptUp;

	SELECTPROC  m_callback;
	LPVOID m_lpParam;

	CClipboard m_Clip;
};

