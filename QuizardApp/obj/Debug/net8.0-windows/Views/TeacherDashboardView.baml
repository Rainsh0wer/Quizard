   M S B A M L   `   `   `  ����E  AQuizardApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:QuizardApp.ViewsQuizardApp.Views  5      +    %QuizardApp.Views.TeacherDashboardView   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      0localclr-namespace:QuizardApp.Views     5      .��5	      �� $
��Auto=�6   .��5
   
   � $��20q�6     � RowDefinitions  5      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $��* �6    .+�5      �� $��0��6   ���	#FF6F42C1 �� CornerRadius$ 8��69    �� Padding$ 20q�6J   $��0,0,0,20q�6W   ._�5      �� .+�5      �� $��24(�5       $
��Bold"�5       
���������5       �� *WelcomeMessage+5       �� 5      $6��0Manage your quizzes and monitor student progress��6    $��14(�5       
������� J� Opacity$	 0.9Q�5       5      5      � 5       $��1��6   $��0,0,0,20q�6     � ColumnDefinitions 5!      �� 5"      $��* �6'   �� 5#      $��* �6'   �� 5$      $��* �6'   �� 5%      $��* �6'   .+�5(      �� $��0��6   ���	#FFF8F9FA���	#FFdee2e6$��1q�6X   $ 4��6l   $ 15q�6}   $��0,0,10,0q�6�   ._�5)      �� $��Center=�6!   .+�5*      �� $��32(�6B   $
��Bold"�6P   ���	#FF007ACC$��Center=�6y   ��6$   �� *TotalQuizzes+6y   �� 5+      $��
My Quizzes��6$   $��12(�66   ���	#FF6C757D$��Center=�6[   5,      5-      �� 5/      $��1��6   ���	#FFF8F9FA���	#FFdee2e6$��1q�6X   $ 4��6l   $ 15q�6}   $��0,0,10,0q�6�   ._�50      �� $��Center=�6!   .+�51      �� $��32(�6B   $
��Bold"�6P   ���	#FF28A745$��Center=�6y   ��6$   �� *TotalClasses+6y   �� 52      $��
My Classes��6$   $��12(�66   ���	#FF6C757D$��Center=�6[   53      54      �� 56      $��2��6   ���	#FFF8F9FA���	#FFdee2e6$��1q�6X   $ 4��6l   $ 15q�6}   $��0,0,10,0q�6�   ._�57      �� $��Center=�6!   .+�58      �� $��32(�6C   $
��Bold"�6Q   ���	#FFFD7E14$��Center=�6z   ��6$   �� *TotalStudents+6z   �� 59      $��Total Students��6$   $��12(�6:   ���	#FF6C757D$��Center=�6_   5:      5;      �� 5=      $��3��6   ���	#FFF8F9FA���	#FFdee2e6$��1q�6X   $ 4��6l   $ 15q�6}   ._�5>      �� $��Center=�6!   .+�5?      �� $��32(�6J   $
��Bold"�6X   ���	#FF6F42C1$��Center=�6�   ��6$   �� *QuizzesTakenThisWeek+6�   �� 5@      $��Quizzes This Week��6$   $��12(�6=   ���	#FF6C757D$��Center=�6b   5A      5B      5C      � 5F      $��2��6    5G      �� 5H      $��2* �6'   �� 5I      $��* �6'   .+�5M      �� $��0��6   
����������	#FFdee2e6$��1q�6T   $ 8��6h   $ 20q�6y   $��0,0,10,0q�6�   ._�5N      �� .+�5O      � $��0,0,0,15q�6    5P      �� 5Q   "   $��* �63   �� 5R   "   $
��Auto �63   �� 5S   "   $
��Auto �63   .+�5U      �� $��0��6(   $��📝 My Quizzes��68   $��18(�6M   $
��Bold"�6[   �� 5V      $��1��6%   $��➕ Create Quiz��65   ���	#FF28A745
�������$	��8,4q�5W   O   $��0q�6]   $��12(�6q   $��0,0,10,0q�6   ��5V   M   �� *CreateQuizCommand+5W      �� 5X      $��2��6%   $��🔄 Refresh��65   ���	#FF17A2B8
�������$	��8,4q�5Y   O   $��0q�6]   $��12(�6q   ��5X   J   �� *RefreshCommand+5Y   q   5Z      �� 5\      $	��400��6E   ��6#   �� *	MyQuizzes+��5]      �� 5^   "   .R�5_   &   �� ���	#FFF8F9FA$ 4��6D   $ 12q�6U   $	��0,2q�6b   ._�5`   *   �  5a   .   �� 5b   2   $��* �6C   �� 5c   2   $
��Auto �6C   �� 5d   2   $
��Auto �6C   .+�5g   .   �� $��0��69   .+�5h   2   �� $��SemiBold"�6S   ��6<   �� *Title+6S   �� 5i   2   $��12(�6Y   ���	#FF6C757D$��CharacterEllipsis=�6~   ��6<   �� *Description+6~   �� 5j   2    �� Orientation$ 
Horizontal=�6=   .+�5k   6   �� $��11(�6y   ���	#FF007ACC$��0,0,15,0q�6�   ��6@   �� *Subject.Name+ �� StringFormat$ Subject: {0}��6�   �� 5l   k   $��11(�6�   ���	#FF6C757D��6u   �� *	CreatedAt+$ Created: {0:MMM dd}��6�   5m   3   5n   /   �� 5p   .   $��1��65   $��✏️ Edit��5q   5   ���	#FFFD7E14
�������$	��8,4q�5v   5   $��0q�5w   5   $��12(�5x   5   $��0,0,5,0q�5y   5   ��5r   5   �� *DataContext.EditQuizCommand+ �� RelativeSource ��  �� AncestorType ListView��5s   5   �� 5y   5   �� 5{   .   $��2��65   $��🗑️ Delete��5|   5   ���	#FFDC3545
�������$	��8,4q�5�   5   $��0q�5�   5   $��12(�5�   5   ��5}   5   �� *DataContext.DeleteQuizCommand+ ��  ListView��5~   5   �� 5�   5   5�   +   5�   '   5�   #   5�      �� 5�      $
��0,20q�6$   �� 5�      $��📊 Quiz Statistics��6$   $��16(�6>   $
��Bold"�6L   $��0,0,0,10q�6^   �� 5�      $	��250��6J   ��6#   �� *QuizStatistics+��5�      �� 5�   "   .R�5�   &   �� ���	#FFF0F8FF$ 4��6D   $ 10q�6U   $	��0,2q�6b   ._�5�   *   �  5�   .   �� 5�   2   $��* �6C   �� 5�   2   $
��Auto �6C   �� 5�   2   $
��Auto �6C   �� 5�   2   $
��Auto �6C   .+�5�   .   �� $��0��68   $��SemiBold"�6d   $��12(�6z   ��6H   �� *
Quiz.Title+6z   �� 5�   .   $��1��68   $��11(�6�   ���	#FF007ACC$
��10,0q�6�   ��6H   �� *TotalAttempts+$ {0} attempts��6�   �� 5�   .   $��2��68   $��11(�6j   ���	#FF28A745$
��10,0q�6�   ��6H   �� *AverageScoreText+6�   �� 5�   .   $��3��68   $��11(�6g   ���	#FF6C757D��6H   �� *LastTakenText+6u   5�   +   5�   '   5�   #   5�      5�      5�      �� 5�      $��1��6   .+�5�      �� 
����������	#FFdee2e6$��1q�6H   $ 8��6\   $ 20q�6m   $��0,0,0,15q�6z   ._�5�      �� .+�5�      �� $��🚀 Quick Actions��6(   $��16(�6@   $
��Bold"�6N   $��0,0,0,15q�6`   �� 5�      $��📊 View All Results��6%   ���	#FF007ACC
�������$
��12,8q�5�   %   $��0q�5�   %   $��0,0,0,10q�5�   %   ��5�   %   �� *ViewResultsCommand+5�   %   �� 5�      $��🎓 Manage Classes��6%   ���	#FF6F42C1
�������$
��12,8q�5�   %   $��0q�5�   %   $��0,0,0,10q�5�   %   ��5�   %   �� *ViewClassesCommand+5�   %   �� 5�      $��🔍 Search Subjects��6%   ���	#FF17A2B8
�������$
��12,8q�5�   %   $��0q�5�   %   ��5�   %   �� *SearchSubjectsCommand+5�   %   5�      5�      �� 5�      
����������	#FFdee2e6$��1q�6H   $ 8��6\   $ 20q�6m   $��0,0,0,15q�6z   ._�5�      �� .+�5�      �� $��🎓 My Classes��6(   $��16(�6=   $
��Bold"�6K   $��0,0,0,15q�6]   �� 5�      $	��150��6I   ��6'   �� *	MyClasses+��5�   "   �� 5�   &   .R�5�   *   �� ���	#FFF8F9FA$ 4��6H   $ 8q�6Y   $	��0,2q�6e   ._�5�   .   �� .+�5�   2   �� $��SemiBold"�6R   $��12(�6h   ��6<   �� *Name+6h   �� 5�   2   $��10(�6~   ���	#FF6C757D��6<   �� *Enrollments.Count+$ {0} students��6�   �� 5�   2   $��10(�6Y   ���	#FF6C757D$��CharacterEllipsis=�6~   ��6<   �� *Description+6~   5�   /   5�   +   5�   '   5�      5�      5�      �� 5�      
����������	#FFdee2e6$��1q�6H   $ 8��6\   $ 20q�6m   ._�5�      �� .+�5�      �� $��📈 Recent Activity��6(   $��16(�6B   $
��Bold"�6P   $��0,0,0,15q�6b   �� 5�      $	��200��6M   ��6'   �� *RecentResults+��5�   "   �� 5�   &   .R�5�   *   �� ���	#FFF8F9FA$ 4��6H   $ 8q�6Y   $	��0,2q�6e   ._�5�   .   �� .+�5�   2   �� $��SemiBold"�6^   $��12(�6t   ��6<   �� *Student.FullName+6t   �� 5�   2   $��11(�6X   ���	#FF007ACC��6<   �� *
Quiz.Title+6f   �� 5�   2   $ 
Horizontal=�6=   .+�5�   6   �� $��10(�6t   ���	#FF28A745$��0,0,10,0q�6�   ��6@   �� *Score+$ Score: {0:F1}%��6�   �� 5�   6   $��10(�6z   ���	#FF6C757D��6@   �� *CompletedAt+$ 
{0:MMM dd}��6�   5�   3   5�   /   5�   +   5�   '   5�      5�      5�      5�      5�      � 5�      $��3��6   ���	#88000000y�6;   �� *	IsLoading+3   -QuizardApp.Views.BooleanToVisibilityConverter	   Instance
 �� 	Converter#
 Z	 .+�5�      �� 
�������$ 8��6,   $ 20q�6=   $��Center=�6J   $��Center=�6g   ._�5�      �� $��Center=�6!   .+�5�      �  � IsIndeterminate$
 True��6&   $	��200��6=   $��4��6I   $��0,0,0,10q�6T   �� 5�      $��
Loading...��6$   $��14(�66   $��Center=�6D   5�      5�      5�      5�      5�      5�      