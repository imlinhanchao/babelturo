#include "StdAfx.h"
#include "Request.h"

#include <afxinet.h>
#include <Wininet.h>
#pragma comment(lib, "Wininet.lib")


namespace Lib {

Request::Request(void)
{

}

Request::~Request(void)
{

}

Response Request::Get( CString sUrl )
{
	Response rsp;
	Shell shell;
	shell.Execute(Path::GetCurDirectory(_T("get.exe")), sUrl, Shell::CONSOLE, false);
	while(shell.IsRunning()) Sleep(100);
	rsp.sContent = shell.GetOutput();

	return rsp;
}

}