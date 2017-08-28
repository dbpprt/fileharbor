$cmd = {
  for ($i = 0; $i -lt 5000; $i++) { X:\Development\Go\src\github.com\dennisbappert\fileharbor\tests.ps1 }
}

$foo = "foo"

1..25 | ForEach-Object {
  Start-Job -ScriptBlock $cmd -ArgumentList $_, $foo
}