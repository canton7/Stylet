require 'albacore'

NUNIT_CONSOLE = Dir['packages/NUnit.Runners.*/tools/nunit-console.exe'].first

msbuild :debug do |b|
  b.solution = "Stylet.sln"
  b.targets = [:Build]
  b.properties = {:Configuration => "Debug"}
end

build :stylet_debug do |b|
  b.file = "Stylet/Stylet.csproj"
  b.target = ['Build']
  b.prop 'Configuration', 'Debug'
end

build :stylet_unit_tests do |b|
  b.file = 'StyletUnitTests/StyletUnitTests.csproj'
  b.target = ['Build']
  b.prop 'Configuration', 'Debug'
end

#nunit :test => [:stylet_debug, :stylet_unit_tests] do |t|
#  t.command = NUNIT_CONSOLE
#  t.assemblies = 'StyletUnitTests/bin/Debug/StyletUnitTests.dll'
#  t.parameters = ['/nologo']
#end

#../packages/OpenCover.4.5.2506/OpenCover.Console.exe -register:user -target:C:\Program Files (x86)\NUnit 2.6.3\bin\nunit-console.exe -filter:+[*]* -targetargs:bin/Debug/StyletUnitTests.dll /noshadow -output:output.xml

