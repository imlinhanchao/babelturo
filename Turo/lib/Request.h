#pragma once

#include "Base.h"

namespace Lib {

typedef struct _Response {
	CString sContent;
	int nStatusCode;

	_Response(){
		nStatusCode = 200;
	}

}Response, *PResponse;

class Request : public Base
{
public:
	Request(void);
	~Request(void);

	static Response Get(CString sUrl);
	static CString UrlEncode(CString sIn);
	
};

}
