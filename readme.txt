plik exe w debug uruchamia si� przez wiersz polece� z takimi parametrami
sekwencjonowanie_DNA.exe -f input.txt -a

parametry:
-f lokalizacja_pliku , lokalizacja pliku z danymi wej�ciowymi.
-r liczba_iteracji , wypisanie rozwi�za� u�ywaj�c losowego wyboru �cie�ki eulera, liczba iteracji okre�la ile b�dzie pr�b przej�cia �cie�ki.
-a , wypisanie wszystkich rozwi�za�.
-t wielko��_pods�owa , tryb testowego wej�cia, w pliku -f musi by� podana wy��cznie sekwencja wynikowa, program sam sobie na podstawie tego generuje plik wej�ciowy i zapisuje go pod nazw� testInput.txt, algorytm jest uruchamiany na wygenerowanych danych.

Brak parametru -r oraz -a oznacza wypisanie jednego rozwi�zania.

Uwagi:
Tryby -r 1000 i -a powinny zwr�ci� dok�adnie takie same wyniki, je�eli tryb losowy zwr�ci� wi�cej to oznacza, �e strategia wybierania dr�g dla �cie�ek w algorytmie jest z�a.