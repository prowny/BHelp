Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { Remove-Item $_.FullName -Force -Recurse }
Get-ChildItem packages\ | foreach ($_) { Remove-Item $_.FullName -Force -Recurse }
