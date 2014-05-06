require 'albacore'

CONFIG = ENV['CONFIG'] || 'Debug'

NUNIT_TOOLS = Dir['packages/NUnit.Runners.*/tools'].first
NUNIT_CONSOLE = File.join(NUNIT_TOOLS, 'nunit-console.exe')
NUNIT_EXE = File.join(NUNIT_TOOLS, 'nunit.exe')

OPENCOVER_CONSOLE = Dir['packages/OpenCover.*/OpenCover.Console.exe'].first
REPORT_GENERATOR = Dir['packages/ReportGenerator*/ReportGenerator.exe'].first

UNIT_TESTS_DLL = "StyletUnitTests/bin/#{CONFIG}/StyletUnitTests.dll"

COVERAGE_DIR = 'Coverage'

desc "Build Stylet.sln using the current CONFIG (or Debug)"
build :build do |b|
  b.sln = "Stylet.sln"
  b.target = [:Build]
  b.prop 'Configuration', CONFIG
end

desc "Run unit tests using the current CONFIG (or Debug)"
test_runner :test => [:build] do |t|
  t.exe = NUNIT_CONSOLE
  t.files = [UNIT_TESTS_DLL]
  t.add_parameter '/nologo'
end

desc "Launch the NUnit gui pointing at the correct DLL for CONFIG (or Debug)"
task :nunit do |t|
  sh NUNIT_EXE, UNIT_TESTS_DLL
end

desc "Generate code coverage reports for CONFIG (or Debug)"
task :cover => [:build] do |t|
  sh OPENCOVER_CONSOLE, %Q{-register:user -target:"#{NUNIT_CONSOLE}" -filter:+[*]* -targetargs:"#{UNIT_TESTS_DLL} /noshadow" -output:"#{File.join(COVERAGE_DIR, 'coverage.xml')}"}
  sh REPORT_GENERATOR, %Q{-reports:"coverage.xml" -targetdir:"#{COVERAGE_DIR}"}
end

