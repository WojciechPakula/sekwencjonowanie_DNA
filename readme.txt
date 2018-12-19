plik exe w debug uruchamia siê przez wiersz poleceñ z takimi parametrami
sekwencjonowanie_DNA.exe -f input.txt -a

parametry:
-f lokalizacja_pliku , lokalizacja pliku z danymi wejœciowymi.
-r liczba_iteracji , wypisanie rozwi¹zañ u¿ywaj¹c losowego wyboru œcie¿ki eulera, liczba iteracji okreœla ile bêdzie prób przejœcia œcie¿ki.
-a , wypisanie wszystkich rozwi¹zañ.

Brak parametru -r oraz -a oznacza wypisanie jednego rozwi¹zania.

Uwagi:
Tryby -r 1000 i -a powinny zwróciæ dok³adnie takie same wyniki, je¿eli tryb losowy zwróci³ wiêcej to oznacza, ¿e strategia wybierania dróg dla œcie¿ek w algorytmie jest z³a.