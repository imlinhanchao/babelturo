#include "stdafx.h"
#include "Translator.h"
#include "../lib/json/json.h"
#ifdef _DEBUG
#pragma comment(lib, "lib/json/lib_jsond.lib")
#else
#pragma comment(lib, "lib/json/lib_json.lib")
#endif

CString CTranslator::m_sTtk = _T("429175.1243284773");

void CTranslator::Init(void)
{
	Response rsp = Request::Get(_T("https://translate.google.cn/"));
	CString sContent = rsp.sContent;
	sContent = sContent.Mid(sContent.Find(_T("tkk:'")) + 5);
	sContent = sContent.Mid(0, sContent.Find(_T("'")));
	m_sTtk = sContent;
}

TRANSLATE CTranslator::get( CString sWord, CString sFrom, CString sTo )
{
	CString sUrl = _T("https://translate.google.cn/translate_a/single");
	sUrl += _T("?client=webapp&sl=") + sFrom;
	sUrl += _T("&tl=") + sTo + _T("&tk=&tk=") + tk(sWord, m_sTtk) + _T("&q=") + sWord;
	sUrl += _T("&hl=zh-CN&dt=at&dt=bd&dt=ex&dt=ld&dt=md&dt=qca&dt=rw&dt=rm&dt=ss&dt=t&pc=1&otf=1&ssel=0&tsel=0&kc=1");
	Response rsp = Request::Get(sUrl);
	CString sContent = rsp.sContent;
	
	return parse(sContent, sTo);
}

TRANSLATE CTranslator::parse(CString sContent, CString sTo)
{
	TRANSLATE result;
	USES_CONVERSION;
	std::string sText_s(T2A(sContent));

	Json::Reader reader;
	Json::Value data;

	if(!reader.parse(sText_s, data))
	{
		std::string sError_s= reader.getFormatedErrorMessages();
#ifdef _DEBUG
		AfxMessageBox(CString(sError_s.c_str()));
#endif
		return result;
	}

	result.sResult = _T("");
	result.sFrom = CString(data[2].asString().c_str());
	result.sTo = sTo;
	data = data[0];

	int nCount = data.size() - 1;
	for (int i = 0; i < nCount; i++)
	{
		result.sResult += CString(data[i][0].asString().c_str());
	}

	return result;
}

CString CTranslator::tk( CString sText, CString tkk )
{
	CString c = _T("&tk=");
	int* e = new int[sText.GetLength() * 5];
	int f = 0;
	for (int g = 0; g < sText.GetLength(); g++) {
		int k = sText[g];
		if (128 > k) e[f++] = k;
		else if(2048 > k) e[f++] = k >> 6 | 192;
		else {
			if (55296 == (k & 64512) && g + 1 < sText.GetLength() && 56320 == (sText[g + 1] & 64512))
			{
				k = 65536 + ((k & 1023) << 10) + (sText[++g] & 1023);
				e[f++] = k >> 18 | 240;
				e[f++] = k >> 12 & 63 | 128;
			}
			else
			{
				e[f++] = k >> 12 | 224;
				e[f++] = k >> 6 & 63 | 128;
			}
			e[f++] = k & 63 | 128;
		}
	}
	int b = _ttoi(Cvt::Split(tkk, '.', 0));
	long a = b;
	for (int i = 0; i < f; i++){
		a += e[i];
		a = Io(a, _T("+-a^+6"));
	}
	a = Io(a, _T("+-3^+b+-f"));
	a ^= _ttoi(Cvt::Split(tkk, '.', 1)) ? _ttoi(Cvt::Split(tkk, '.', 1)) : 0;
	if(0 > a) a = ((a & 2147483647L) % 1000000 + 2147483648L % 1000000L) % 1000000;
	else a = a % 1000000L;
	delete[] e;
	return Cvt::ToString(a) + _T(".") + Cvt::ToString(a ^ b);
}

int CTranslator::Io( int a, CString b )
{
	for (int c = 0; c < b.GetLength() - 2; c += 3) {
		int d = b[c + 2];
		d = 'a' <= d ? d - 87 : Cvt::ToNumber(d);
		d = '+' == b[c + 1] ? ((unsigned int)a) >> d : a << d;
		a = '+' == b[c] ? a + d & 4294967295 : a ^ d;
	}
	return a;
}

