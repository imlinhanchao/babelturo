#pragma once

#include "resource.h"

// CResultDlg dialog

class CResultDlg : public CDialogEx
{
	DECLARE_DYNAMIC(CResultDlg)

public:
	CResultDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CResultDlg();

// Dialog Data
	enum { IDD = IDD_RESULT_DLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()

public:
	void SetResult(TRANSLATE result);
};
