   M S B A M L   `   `   `  ����E  AQuizardApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:QuizardApp.ViewsQuizardApp.Views  5   	   !    QuizardApp.Views.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:QuizardApp.Views     5   	     � Title$   Quizard - Quiz Application��5   	   $	��800��5	   	   $
��1200��6   $	��600��5
   	   $	��800��6    � WindowStartupLocation$ CenterScreen=�5   	    � WindowState$ 	Maximized=�5   	    � 	Resources 5       �� MergedDictionaries �� Source   ModernButtonStyle �� 
TargetType �� Property �� Value	 �� Triggers
 P� Property P� Value  StatusBarStyle  ToolBarStyle�� 5   
    5      �� 5      $ ViewTemplates.xaml?�6%   %�  &	        &	 v    &	     �� 5   
   " ��6*   .�5      ��  � �� �	#FF007ACC6+   �� 5       � ��
 �����6+   �� 5       � ��$ SemiBold"�6+   �� 5       � ��$
 12,8q�6(   �� 5       � ��$ 0q�60   �� 5       �@�Cursor$
 Hand��6'   	 5      P� 5      
 �@J�IsMouseOver$
 True��61   . �5      ��  � �� �	#FF005A9E63   5       P� 5!      
 � ��$
 True��6/   . �5"      ��  � �� �	#FF00457863   5#      P� 5$      
 � {�$ False��6/   . �5%      ��  � �� �	#FFCCCCCC63   �� 5&       � �� �	#FF66666663   5'      5)      �� 5+   
   " ��6'   .�5,      ��  � �� �	#FF2D2D306+   �� 5-       � ��
 �����6+   �� 5.       � ��$ 25��6'   5/      �� 51   
   " c�6%   .�52      ��  � �� �	#FF3E3E426+   �� 53       � ��$ 40��6'   54      55      .��58      �  � RowDefinitions 59   
   �� 5:      $
��Auto �6   �� 5;      $��* �6   �� 5<      $
��Auto �6   .+�5@   
   c� $��0��6   #��[ 6   .@�5A      �� $��	🏠 Home��6   #��[  5B      ��5C      �� *NavigateCommand+��5D      �� *CurrentUser.Role+/   )QuizardApp.Views.RoleToDashboardConverter   Instance �� 	Converter# Z y�5E      �� *
IsLoggedIn+3   -QuizardApp.Views.BooleanToVisibilityConverter   Instance# Z �� 5G      y�6   �� *
IsLoggedIn+# Z �� 5J       �� Orientation$ 
Horizontal=�6   y�5K      �� *	IsStudent+# Z .+�5L      �� $��📝 Take Quiz��6   #��[  5M      $��TakeQuiz��5O      ��5N      �� *NavigateCommand+5O      �� 5P      $��📊 My Results��6   #��[  5Q      $��ViewResults��5S      ��5R      �� *NavigateCommand+5S      �� 5T      $��🔍 Search Subjects��6   #��[  5U      $��SearchSubjects��5W      ��5V      �� *NavigateCommand+5W      �� 5X      $��🎓 Join Class��6   #��[  5Y      $��	JoinClass��5[      ��5Z      �� *NavigateCommand+5[      5\      �� 5_      $ 
Horizontal=�6   y�5`      �� *	IsTeacher+# Z .+�5a      �� $��➕ Create Quiz��6   #��[  5b      $��
CreateQuiz��5d      ��5c      �� *NavigateCommand+5d      �� 5e      $��📊 View Results��6   #��[  5f      $��ViewResults��5h      ��5g      �� *NavigateCommand+5h      �� 5i      $��🏫 My Classes��6   #��[  5j      $��ViewClasses��5l      ��5k      �� *NavigateCommand+5l      �� 5m      $��🔍 Browse Subjects��6   #��[  5n      $��SearchSubjects��5p      ��5o      �� *NavigateCommand+5p      5q      �� 5t      $ 
Horizontal=�6   $��Right=�62   .+�5u      �� $��⬅️ Back��6   #��[  5v      ��5w      �� *GoBackCommand+y�5x      �� *GoBackCommand+3   -QuizardApp.Views.CommandToVisibilityConverter   Instance# Z �� 5z      
�������$��Center=�5|      $
��10,0q�5}      ��5z      �� *CurrentUser+4   .QuizardApp.Views.UserToWelcomeMessageConverter   Instance# Z y�5~      �� *
IsLoggedIn+# Z �� 5�      $��🚪 Logout��6   #��[  5�      ��5�      �� *LogoutCommand+y�5�      �� *
IsLoggedIn+# Z 5�      5�      � 5�   
   $��1��6   ���	#80000000 K� ZIndex$	 999��5�      y�5�      �� *	IsLoading+# Z .+�5�      �� $��Center=�6   $��Center=�66   .+�5�      �  � IsIndeterminate$
 True��6   $	��200��65   $��20��6A   $��0,0,0,10q�6M   �� 5�      $��
Loading...��6   
�������$��Center=�6A   $��16(�6^   5�      5�      �� 5�   
   -   5         ��Name$ MainContent��5�      $��1��5�      ��5�      �� *CurrentViewModel+��5�      0   *QuizardApp.Views.ViewModelTemplateSelector  5�      5�      �� 5�   
   $��2��6   #��[ 6!   .@�5�      �� .��5�      �� $��12(�6\   ��6   �� *CurrentState+ �� StringFormat$ Current View: {0}��6\   5�      �� 5�      $��Right=�6   .��5�      �� $��12(�6;   $
��Bold"�6I   ��6   �� *StatusMessage+6I   5�      5�      5�      5�      