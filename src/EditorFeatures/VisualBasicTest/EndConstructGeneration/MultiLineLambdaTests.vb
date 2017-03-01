' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.


Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.EndConstructGeneration
    Public Class MultiLineLambdaTests
        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Sub TestApplyWithFunctionLambda()
            VerifyStatementEndConstructApplied(
                before:="Class c1
  Sub foo()
    Dim x = Function()
  End Sub
End Class",
                beforeCaret:={2, -1},
                after:="Class c1
  Sub foo()
    Dim x = Function()

            End Function
  End Sub
End Class",
                afterCaret:={3, -1})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Sub TestApplyWithFunctionLambdaWithMissingEndFunction()
            VerifyStatementEndConstructApplied(
                before:="Class c1
  Function foo()
    Dim x = Function()
End Class",
                beforeCaret:={2, -1},
                after:="Class c1
  Function foo()
    Dim x = Function()

            End Function
End Class",
                afterCaret:={3, -1})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Sub TestApplyWithSubLambda()
            VerifyStatementEndConstructApplied(
                before:="Class c1
  Function foo()
    Dim x = Sub()
  End Function
End Class",
                beforeCaret:={2, -1},
                after:="Class c1
  Function foo()
    Dim x = Sub()

            End Sub
  End Function
End Class",
                afterCaret:={3, -1})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration), WorkItem(544362, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544362")>
        Public Sub TestApplyWithSubLambdaWithNoParameterParenthesis()
            VerifyStatementEndConstructApplied(
                before:="Class c1
  Function foo()
    Dim x = Sub
  End Function
End Class",
                beforeCaret:={2, -1},
                after:="Class c1
  Function foo()
    Dim x = Sub()

            End Sub
  End Function
End Class",
                afterCaret:={3, -1})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration), WorkItem(544362, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544362")>
        Public Sub TestApplyWithSubLambdaInsideMethodCall()
            VerifyStatementEndConstructApplied(
                before:="Class c1
  Function foo()
    M(Sub())
  End Function
End Class",
                beforeCaret:={2, 11},
                after:="Class c1
  Function foo()
    M(Sub()

      End Sub)
  End Function
End Class",
                afterCaret:={3, -1})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration), WorkItem(544362, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544362")>
        Public Sub TestApplyWithSubLambdaAndStatementInsideMethodCall()
            VerifyStatementEndConstructApplied(
                before:="Class c1
  Function foo()
    M(Sub() Exit Sub)
  End Function
End Class",
                beforeCaret:={2, 11},
                after:="Class c1
  Function foo()
    M(Sub()
          Exit Sub
      End Sub)
  End Function
End Class",
                afterCaret:={3, 10})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration), WorkItem(544362, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/544362")>
        Public Sub TestApplyWithFunctionLambdaInsideMethodCall()
            VerifyStatementEndConstructApplied(
                before:="Class c1
  Function foo()
    M(Function() 1)
  End Function
End Class",
                beforeCaret:={2, 17},
                after:="Class c1
  Function foo()
    M(Function()
          Return 1
      End Function)
  End Function
End Class",
                afterCaret:={3, 17})
        End Sub

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function TestVerifyAnonymousType() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s()
        Dim x = New With {.x = Function(x)
    End Sub
End Class",
                beforeCaret:={2, -1},
                 after:="Class C
    Sub s()
        Dim x = New With {.x = Function(x)

                               End Function
    End Sub
End Class",
                afterCaret:={3, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function VerifySingleLineLambdaFunc() As Threading.Tasks.Task
            VerifyStatementEndConstructNotApplied(
                text:="Class C
    Sub s()
        Dim x = Function(x) x
    End Sub
End Class",
                caret:={2, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function VerifySingleLineLambdaSub() As Threading.Tasks.Task
            VerifyStatementEndConstructNotApplied(
                text:="Class C
    Sub s()
        Dim y = Sub(x As Integer) x.ToString()
    End Sub
End Class",
                caret:={2, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function TestVerifyAsDefaultParameterValue() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s(Optional ByVal f As Func(Of String, String) = Function(x As String)
End Class",
                beforeCaret:={1, -1},
                 after:="Class C
    Sub s(Optional ByVal f As Func(Of String, String) = Function(x As String)

                                                        End Function
End Class",
                afterCaret:={2, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function TestVerifyNestedLambda() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    sub s
        Dim x = Function (x)
                    Dim y = function(y)
                End Function
    End sub
End Class",
                beforeCaret:={3, -1},
                 after:="Class C
    sub s
        Dim x = Function (x)
                    Dim y = function(y)

                            End function
                End Function
    End sub
End Class",
                afterCaret:={4, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function TestVerifyInField() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Dim x = Sub()
End Class",
                beforeCaret:={1, -1},
                 after:="Class C
    Dim x = Sub()

            End Sub
End Class",
                afterCaret:={2, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function VerifyInvalidLambdaSyntax() As Threading.Tasks.Task
            VerifyStatementEndConstructNotApplied(
                text:="Class C
    Sub s()
        Sub(x)
    End Sub
End Class",
                caret:={2, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function VerifyNotAppliedIfSubLambdaContainsEndSub() As Threading.Tasks.Task
            VerifyStatementEndConstructNotApplied(
                text:="Class C
    Sub s()
        Dim x = Sub() End Sub
    End Sub
End Class",
                caret:={2, 21})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function VerifyNotAppliedIfSyntaxIsFunctionLambdaContainsEndFunction() As Threading.Tasks.Task
            VerifyStatementEndConstructNotApplied(
                text:="Class C
    Sub s()
        Dim x = Function() End Function
    End Sub
End Class",
                caret:={2, 26})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function VerifyLambdaWithImplicitLC() As Threading.Tasks.Task
            VerifyStatementEndConstructNotApplied(
                text:="Class C
    Sub s()
        Dim x = Function(y As Integer) y +
    End Sub
End Class",
                caret:={2, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function TestVerifyLambdaWithMissingParenthesis() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s()
        Dim x = Function
    End Sub
End Class",
                   beforeCaret:={2, -1},
                   after:="Class C
    Sub s()
        Dim x = Function()

                End Function
    End Sub
End Class",
                   afterCaret:={3, -1})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function TestVerifySingleLineSubLambdaToMultiLine() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s()
        Dim x = Sub() f()
    End Sub
End Class",
                   beforeCaret:={2, 21},
                   after:="Class C
    Sub s()
        Dim x = Sub()
                    f()
                End Sub
    End Sub
End Class",
                   afterCaret:={3, 20})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration), WorkItem(530683, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530683")>
        Public Async Function TestVerifySingleLineSubLambdaToMultiLineWithTrailingTrivia() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s()
        Dim x = Sub() f() ' Invokes f()
    End Sub
End Class",
                   beforeCaret:={2, 21},
                   after:="Class C
    Sub s()
        Dim x = Sub()
                    f() ' Invokes f()
                End Sub
    End Sub
End Class",
                   afterCaret:={3, 20})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration)>
        Public Async Function TestVerifySingleLineFunctionLambdaToMultiLine() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s()
        Dim x = Function() f()
    End Sub
End Class",
                   beforeCaret:={2, 27},
                   after:="Class C
    Sub s()
        Dim x = Function()
                    Return f()
                End Function
    End Sub
End Class",
                   afterCaret:={3, 27})
        End Function

        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration), WorkItem(530683, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530683")>
        Public Async Function TestVerifySingleLineFunctionLambdaToMultiLineWithTrailingTrivia() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s()
        Dim x = Function() 4 ' Returns Constant 4
    End Sub
End Class",
                   beforeCaret:={2, 27},
                   after:="Class C
    Sub s()
        Dim x = Function()
                    Return 4 ' Returns Constant 4
                End Function
    End Sub
End Class",
                   afterCaret:={3, 27})
        End Function

        <WorkItem(1922, "https://github.com/dotnet/roslyn/issues/1922")>
        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration), WorkItem(530683, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530683")>
        Public Async Function TestVerifySingleLineFunctionLambdaToMultiLineInsideXMLTag() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s()
        Dim x = <xml><%= Function()%></xml>
    End Sub
End Class",
                   beforeCaret:={2, 35},
                   after:="Class C
    Sub s()
        Dim x = <xml><%= Function()

                         End Function %></xml>
    End Sub
End Class",
                   afterCaret:={3, -1})
        End Function

        <WorkItem(1922, "https://github.com/dotnet/roslyn/issues/1922")>
        <WpfFact, Trait(Traits.Feature, Traits.Features.EndConstructGeneration), WorkItem(530683, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530683")>
        Public Async Function TestVerifySingleLineSubLambdaToMultiLineInsideXMLTag() As Task
            VerifyStatementEndConstructApplied(
                before:="Class C
    Sub s()
        Dim x = <xml><%= Sub()%></xml>
    End Sub
End Class",
                   beforeCaret:={2, 30},
                   after:="Class C
    Sub s()
        Dim x = <xml><%= Sub()

                         End Sub %></xml>
    End Sub
End Class",
                   afterCaret:={3, -1})
        End Function
    End Class
End Namespace
