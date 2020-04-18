#pragma once

typedef struct _TRANSLATE
{
	CString sResult;
	CString sFrom;
	CString sTo;

}TRANSLATE, *PTRANSLATE;

class CTranslator
{
public:
	static void Init(void);
	static TRANSLATE get(CString sWord, CString sFrom, CString sTo);

private:
	static CString tk(CString a, CString tkk);
	static int Io(int a, CString b);
	static TRANSLATE parse(CString sContent, CString sTo);

	static CString m_sTtk;
};