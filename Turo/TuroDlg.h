
// turoDlg.h : header file
//

#pragma once

#include "ResultDlg.h"

// CturoDlg dialog
class CTuroDlg : public CDialogEx
{
// Construction
public:
	CTuroDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_TURO_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnClose();
	DECLARE_MESSAGE_MAP()

	static void CALLBACK SelectProc(POINT ptDown, POINT ptUp, CString sText, LPVOID lpParam);

	CResultDlg m_resultDlg;
public:
	CHook m_Hook;
};
