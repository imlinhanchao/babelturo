// ResultDlg.cpp : implementation file
//

#include "stdafx.h"
#include "Turo.h"
#include "ResultDlg.h"
#include "afxdialogex.h"


// CResultDlg dialog

IMPLEMENT_DYNAMIC(CResultDlg, CDialogEx)

CResultDlg::CResultDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CResultDlg::IDD, pParent)
{

}

CResultDlg::~CResultDlg()
{
}

void CResultDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CResultDlg, CDialogEx)
END_MESSAGE_MAP()


// CResultDlg message handlers


BOOL CResultDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	SetWindowText(_T("Result"));

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE
}

void CResultDlg::SetResult( TRANSLATE result )
{
	SetDlgItemText(IDC_EDIT_RESULT, result.sResult);
}
