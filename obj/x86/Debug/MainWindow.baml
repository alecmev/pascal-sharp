   M S B A M L   `   `   `  ����F  BPascalSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:PascalSharpPascalSharp  5   	       PascalSharp.MainWindow   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   (psclr-namespace:PascalSharp     5   	     � Title$  PascalSharp��5   	   
���   �$	��600��60   $	��800��6=    � WindowState$ 	Maximized=�6I    � WindowStyle$
 None=�6a    � Icon$ PascalSharp.ico��6t   .��5      �� -   5        $��
MainCanvas��5      $��Stretch=�6   $��Stretch=�6=    J� CommandBindings 5      )  #System.Windows.Input.CommandBinding  5   
   -   5           Command$ ps:Commands.Render��5      66     5   
   -   5        $ ps:Commands.Clear��5      65     5   
   -   5        $ ps:Commands.Close��5      65    J� InputBindings 5      �� 5   
    �� Gesture$ CTRL+R��6    �� Command$ ps:Commands.Render��6&   �� 5   
   $ CTRL+C��6   $ ps:Commands.Clear��6&   �� 5   
   
	 �� Key$		 ESC��6   $ ps:Commands.Close��6   
 � ContextMenu
 5      �� 5   
   .@�5      m� -   5        $��Render��5      6'   m� 5      -   5        $��Clear��5      6&   m� 5      -   5        $��Close��5      6&   5      5!      