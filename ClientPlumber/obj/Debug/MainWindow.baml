   M S B A M L   `   `   `  ????H  DClientPlumber, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:ClientPlumberClientPlumber  5   	       ClientPlumber.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:ClientPlumber     5   	   -   5          ? Title$  
Plumber / ??5   	   $	??400??6   $	??800??6)    ? WindowStartupLocation$ CenterScreen=?5	   	    ? DataContext 5      "   ClientPlumber.CellCollection  5   
   .??5      ?  ? ColumnDefinitions 5   
   ?? 5      ?? 5      .+?5   
   ?? ??6   ?? *Cells+??5      ?? 5      .R?5      A? 5      ??5      ?? 5      .R?5      ? .+?5      ?? -   5        ??5       ?? *Image+5        5%      5&      5(      ?? 5)   
   $??1??6   ??6'   ?? *
EnemyCells+??5*      ?? 5+      .R?5,      A? 5-      ??5/      ?? 50      .R?51      ? .+?52      ?? ??6    ?? *Image+53      54      56      ]? 57   
   $??1??6   
??????? J? Opacity$	 0.5Q?59      .+?5:      ?? $
??Left=?6   $??4??5;      
????  ?5<      5=      5>      5?      