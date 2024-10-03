	@echo off
cls
color 08
echo: Witamy w full komilatorze by ELEKTRON
echo:


PATH=%PATH%;"%ProgramFiles%\NEC Electronics Tools\CC78K0S\W2.01\bin\"
PATH=%PATH%;"%ProgramFiles%\NEC Electronics Tools\RA78K0S\W2.00\bin\"
PATH=%PATH%;"%ProgramFiles%\NEC Electronics Tools\CC78K0S\W2.01\"

if exist *.c (echo znaleziono plik/i *.C!) ELSE ( goto no_C )
:: Procedura szukaj¹ca pierwszego pliku z rozszerzeniem C
FOR %%F IN (*.C) DO (
 set filename=%%F
 goto foundC
)

::znaleziono pierwszy plik z rozszerzeniem C
:foundC

::Kompilator C
call cc78k0s -cF9116 -sm -v %filename%
if errorlevel 1 goto BladKompilatora

::Linker
call lk78k0s %filename:~0,-2%.rel startup\s0s.rel -b"%ProgramFiles%\NEC Electronics Tools\CC78K0S\W2.01\lib78k0s\cl0s.lib" -s -o%filename:~0,-2%.lmf -p%filename:~0,-2%.map
if errorlevel 1 goto BladLinkera

:: s0s.rel
:: s0sl.rel
:: s0ss.rel
:: s0ssl.rel

::ObjectConverter
call oc78k0s %filename:~0,-2%
if errorlevel 1 goto BladObjectConvertera
goto succes

:no_C
cls
Color 0e
echo -----------------------WARNING------------------------
echo Nie znaleziono zadnego pliku *.C do kompilacji!
goto end

:BladKompilatora
color 0C
echo -----------------------ERROR--------------------------
Echo Napotkano blad kompilatora C, przerwanie operacji
goto end

:BladLinkera
color 0C
echo -----------------------ERROR--------------------------
Echo Napotkano blad Linkera, przerwanie operacji
goto end

:BladObjectConvertera
color 0C
echo -----------------------ERROR--------------------------
Echo Napotkano blad Object Convertera, przerwanie operacji
goto end

:succes
color 0A
echo -----------------------SUKCES-------------------------
echo: Kompilacja zakonczona sukcesem!
::Czyszczenie œmieci pozosta³ych po kompilacji
Del %filename:~0,-2%.lmf
Del %filename:~0,-2%.rel
Del %filename:~0,-2%.sym
Del %filename:~0,-2%.map

:end
echo ------------------------------------------------------
pause >>nul
