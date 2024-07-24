@echo off
setlocal enabledelayedexpansion

rem -------------------------------------------------------------------------------------
rem Among Us���N������Ă�������擾
rem -------------------------------------------------------------------------------------

set COUNT=0
set LOGFILENAME=LogOutput.log

rem Among Us���N���ς݂��𔻒�
tasklist /FI "IMAGENAME eq Among Us.exe" | find "Among Us.exe" > NUL

rem �N���䐔�̃J�E���g

if not %errorlevel% == 0 (
    echo "���̃E�B���h�E�� [ %LOGFILENAME% ] ���o�͂��Ă��� Among Us.exe �̋N����Ԃ��Ď����Ă��܂��B"
) else (
    set RAN=1
    for /f %%a in ('tasklist /nh /fi "imagename eq Among Us.exe"') do ( 
        set /a COUNT+=1
        set /a RAN+=1
    )
    set LOGFILENAME=LogOutput.!COUNT!.log
    echo "���̃E�B���h�E�� Among Us.exe ( !RAN!��� ) �̋N����Ԃ��Ď����Ă��܂��B"
    echo "�A�v���P�[�V�����I���� [ !LOGFILENAME! ] ���Y���t�H���_�ɑޔ����܂��B"
)

rem -------------------------------------------------------------------------------------

rem -------------------------------------------------------------------------------------
rem �g�p����t�H���_����уt�@�C���p�X�֘A�̕ϐ����쐬 (�J�����g�f�B���N�g�� �g�p Ver.)
rem -------------------------------------------------------------------------------------

rem �N���Ώۂ�Among Us.exe�����݂���t�H���_���w�肷��B
set AMONGUSFOLDER=%~dp0

rem �ȉ��̗l�Ȍ`�� Among Us.exe �����݂���t�H���_���w�肷��`�ɕύX����ƁA���o�b�`�t�@�C����Among Us.exe�ƕʊK�w�ɑ��݂��Ă��A�g�p�\�ɂȂ�
rem set AMONGUSFOLDER=C:\Program Files (x86)\Steam\steamapps\common

set MODFOLDER=%AMONGUSFOLDER%\SuperNewRoles
set AUTOSAVELOGFOLDER=%MODFOLDER%\AutoSaveLogFolder

rem -------------------------------------------------------------------------------------


rem -------------------------------------------------------------------------------------
rem Among Us �N�� & �I���ҋ@
rem -------------------------------------------------------------------------------------

start "" /wait "%AMONGUSFOLDER%\Among Us"

rem -------------------------------------------------------------------------------------


rem -------------------------------------------------------------------------------------
rem �ۑ��t�H���_�����݂��邩�m�F��, ���݂��Ȃ��Ȃ�쐬����B(���ɑ��݂���Ȃ�R�����g�A�E�g����)
rem -------------------------------------------------------------------------------------

if not exist "%AUTOSAVELOGFOLDER%" md "%AUTOSAVELOGFOLDER%"

rem -------------------------------------------------------------------------------------


rem -------------------------------------------------------------------------------------
rem ���O�t�@�C���̍X�V�������擾��, ``yyyyMMdd_hhmm``�̌`�ɕϊ�����
rem -------------------------------------------------------------------------------------

for %%i in ("%AMONGUSFOLDER%\BepInEx\%LOGFILENAME%") do set "UPDATE=%%~ti"
set YYYYMMDD_HHMM=%UPDATE:~0,4%%UPDATE:~5,2%%UPDATE:~8,2%_%UPDATE:~11,2%%UPDATE:~14,2%

rem ���O��ϊ����ăR�s�[����
copy /y "%AMONGUSFOLDER%\BepInEx\%LOGFILENAME%" "%AUTOSAVELOGFOLDER%\%YYYYMMDD_HHMM%_%LOGFILENAME%"
echo "[ %YYYYMMDD_HHMM%_%LOGFILENAME% ] �� �o�͂��܂����B"

rem -------------------------------------------------------------------------------------

rem pause
endlocal


rem -------------------------------------------------------------------------------------
rem # �Q�l

rem ## �v���Z�X�̏�Ԋ֘A�̏���

rem ### �v���Z�X���N���ς݂��̔���
rem - https://cool.japan.ne.jp/win-dos-batch_proc
rem - https://itlogs.net/windows-bat-process-check/

rem ### �v���Z�X�̏�Ԃ̎擾
rem - https://note.com/good_lilac166/n/n0b13bb383737
rem - https://wa3.i-3-i.info/word12514.html

rem ### �w��v���Z�X�̋N���䐔�̊m�F
rem - https://note.com/good_lilac166/n/n0b13bb383737
rem - https://qiita.com/plcherrim/items/9cba5a42273e10915c8f

rem ## �x���W�J
rem - https://qiita.com/talesleaves/items/8990a55b7a770de3d34f#%E6%8B%AC%E5%BC%A7%E3%81%AE%E4%B8%AD%E3%81%A7%E5%A4%89%E6%95%B0%E3%82%92set%E3%81%97%E3%81%9F%E3%82%89%E9%81%85%E5%BB%B6%E5%B1%95%E9%96%8B%E3%82%92%E4%BD%BF%E3%81%86%E3%82%88
rem - https://qiita.com/tana_tomo_1025/items/7f824a154f004f610386

rem ## �A�v���̏I����ҋ@������@
rem - https://qiita.com/talesleaves/items/8990a55b7a770de3d34f#%E5%88%A5%E3%82%A2%E3%83%97%E3%83%AA%E3%82%B1%E3%83%BC%E3%82%B7%E3%83%A7%E3%83%B3%E3%82%92%E8%B5%B7%E5%8B%95%E3%81%95%E3%81%9B%E3%82%8Bstart

rem ## �ŏI�X�V���Ԃ̎擾�ƃt�H�[�}�b�g
rem - https://tekuzo.org/cmd-date-filename/
rem - https://tecsingularity.com/windows/update/

rem ## �f�B���N�g���̊m�F�y�э쐬
rem - https://windows.command-ref.com/cmd-md.html

rem ## �t�@�C���̃R�s�[
rem - https://www.javadrive.jp/command/file/index5.html

rem ## �J�����g�f�B���N�g���̎擾
rem - https://qiita.com/shin1rok/items/efb5052ef5fb8138c26d
rem -------------------------------------------------------------------------------------