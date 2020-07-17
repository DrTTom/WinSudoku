# WinSudoku
Erste Schritte in C# / Sudoku Solver

## Braucht die Welt noch einen Sudoku-Löser?

Sicherlich nicht. Aber ich brauche eine einfache Aufgabe, um C# zu lernen. Aus Tutorials lernt man nur, was für die jeweiligen Autoren 
zu beschreiben naheliegend war. Durch Entwicklung eines Beispielprojekts lernt man, wie man mit der Technik klarkommt.

## Warum ein so komplizierter Algorithmus?

Die Behandlung von 3 Lateinischen Quadraten stammt aus meinem ersten wissenschaftlichen Artikel (Preprint 11/1990 Uni Magdeburg), 
es werden auf sehr schnelle Weise eindeutig bestimmte Elemente gefunden und eingetragen. Dadurch wird das nachgeschaltete einfache Verfahren 
"rate Eintrag und gehe zurück, wenn er nicht passt" auf weniger Einträge eingeschränkt werden. Ein "schweres" Zeitungssudoku wird in durchschnittlich 
2 Schritten gelöst. 

Auf eine analoge Behandlung der möglichen Positionen von Zahlen in den "kleinen Quadraten" und den Algorithmus von Hall wurde verzichtet, da dadurch der 
Gesamtablauf verlangsamt wird, auch wenn mehr eindeutig bestimmte Elemente gefunden werden. 
