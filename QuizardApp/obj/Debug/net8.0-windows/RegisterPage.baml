   M S B A M L   `   `   `  ����  
QuizardApp    QuizardApp.RegisterPage   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5        N� Title$  Register��5       N� 
Background 5	      �� 5
   
    �� ImageSource$* $https://i.ibb.co/zWG5ZhQ1/342342.png��6    j� Stretch$
 Fill=�6H   .��5      � 
������ .+�5   
   �� $	��700��6   $	��280��6    $��600,20,20,20q�6/    �� Padding$	 100q�5      
������� �� CornerRadius$ 10��62   $��Center=�5      $��Center=�6.    J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5         
BlurRadius$ 20Q�6#      ShadowDepth$ 4Q�63   	   Opacity$		 0.3Q�6C   ._�5      �� .+�5      �� $��Create your account��6   $��24(�67   $
��Bold"�6E   $��0,0,0,25q�6W   �� 5      $��Username��6   �� 5      $��0,0,0,10q�6Y   ��6   �� *
Username+
 �� UpdateSourceTrigger$
 PropertyChanged=�6Y   �� 5      $��Email��6   �� 5      $��0,0,0,10q�6V   ��6   �� *Email+$
 PropertyChanged=�6V   �� 5      $��Password��6   G� 5      -   5         G�Name$ passwordBox��5      $��0,0,0,10q�63   �� 5!      $��Confirm Password��6   G� 5"      -   5        $ confirmPasswordBox��5"      $��0,0,0,20q�6:   �� 5$      -   5        $��Register��5$      $��40��5%   	   ���#0F62FE
�������$��SemiBold"�5(   	   $��0q�5)   	   5*   	   �� 5,       �� Orientation$ 
Horizontal=�6   $��Center=�66   $��0,20,0,0q�6S   .+�5-      �� $��Already have an account?��6    �� 5.      $��Sign in��6   
������ ���#0F62FE$��0q�51   	   $��0q�52   	   $��5,0,0,0q�53   	    � Cursor$
 Hand��54   	   ��55   	   �� *GoToLoginCommand+57      58      59      5:      5;      