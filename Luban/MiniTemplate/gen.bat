set WORKSPACE=../..
set GEN_CLIENT=../Luban/Luban.dll

  dotnet %GEN_CLIENT% ^
      -t client ^
      -c cs-bin ^
      -d bin ^
      --conf ./luban.conf ^
      -x outputDataDir=%WORKSPACE%\Assets\Unity\Resources\Luban ^
      -x outputCodeDir=%WORKSPACE%\Assets\SkillEditor\Runtime\Demo\Luban\DataTable ^
      -x tableImporter.valueTypeNameFormat=Table{0}

  dotnet %GEN_CLIENT% ^
      -t editor ^
      -c cs-editor-json ^
      -d json ^
      --conf ./luban.conf ^
      -x outputDataDir=%WORKSPACE%\Assets\Unity\Editor\Luban ^
      -x outputCodeDir=%WORKSPACE%\Assets\SkillEditor/Editor\Demo\Luban\DataTable ^
      -x tableImporter.valueTypeNameFormat=Table{0}
  pause                              
