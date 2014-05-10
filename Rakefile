begin
  require 'albacore'
rescue LoadError
  warn "Please run 'gem install albacore --pre'"
  exit 1
end

CONFIG = ENV['CONFIG'] || 'Debug'

COVERAGE_DIR = 'Coverage'
COVERAGE_FILE = File.join(COVERAGE_DIR, 'coverage.xml')

directory COVERAGE_DIR

desc "Build Stylet.sln using the current CONFIG (or Debug)"
build :build do |b|
  b.sln = "Stylet.sln"
  b.target = [:Build]
  b.prop 'Configuration', CONFIG
end

task :test_environment => [:build] do
  NUNIT_TOOLS = 'packages/NUnit.Runners.*/tools'
  NUNIT_CONSOLE = Dir[File.join(NUNIT_TOOLS, 'nunit-console.exe')].first
  NUNIT_EXE = Dir[File.join(NUNIT_TOOLS, 'nunit.exe')].first

  OPENCOVER_CONSOLE = Dir['packages/OpenCover.*/OpenCover.Console.exe'].first
  REPORT_GENERATOR = Dir['packages/ReportGenerator.*/ReportGenerator.exe'].first

  UNIT_TESTS_DLL = "StyletUnitTests/bin/#{CONFIG}/StyletUnitTests.dll"
  INTEGRATION_TESTS_EXE = "StyletIntegrationTests/bin/#{CONFIG}/StyletIntegrationTests.exe"

  raise "NUnit.Runners not found. Restore NuGet packages" unless NUNIT_CONSOLE && NUNIT_EXE
  raise "OpenCover not found. Restore NuGet packages" unless OPENCOVER_CONSOLE
  raise "ReportGenerator not found. Restore NuGet packages" unless REPORT_GENERATOR
end

test_runner :nunit_test_runner => [:test_environment] do |t|
  t.exe = NUNIT_CONSOLE
  t.files = [UNIT_TESTS_DLL]
  t.add_parameter '/nologo'
end

desc "Run unit tests using the current CONFIG (or Debug)"
task :test => [:nunit_test_runner] do |t|
  rm 'TestResult.xml'
end

desc "Launch the NUnit gui pointing at the correct DLL for CONFIG (or Debug)"
task :nunit do |t|
  sh NUNIT_EXE, UNIT_TESTS_DLL
end


namespace :cover do

  desc "Generate unit test code coverage reports for CONFIG (or Debug)"
  task :unit => [:test_environment, COVERAGE_DIR] do |t|
    coverage(instrument(:nunit, UNIT_TESTS_DLL))
  end

  desc "Create integration test code coverage reports for CONFIG (or Debug)"
  task :integration => [:test_environment, COVERAGE_DIR] do |t|
    coverage(instrument(:exe, INTEGRATION_TESTS_EXE))
  end

  desc "Create test code coverage for everything for CONFIG (or Debug)"
  task :all => [:test_environment, COVERAGE_DIR] do |t|
    coverage([instrument(:nunit, UNIT_TESTS_DLL), instrument(:exe, INTEGRATION_TESTS_EXE)])
  end

end

def instrument(runner, target)
  case runner
  when :nunit
    opttarget = NUNIT_CONSOLE
    opttargetargs = target
  when :exe
    opttarget = target
    opttargetargs = ''
  else
    raise "Unknown runner #{runner}"
  end
 
  coverage_file = File.join(COVERAGE_DIR, File.basename(target).ext('xml'))
  sh OPENCOVER_CONSOLE, %Q{-register:user -target:"#{opttarget}" -filter:"+[Stylet]* -[Stylet]XamlGeneratedNamespace.*" -targetargs:"#{opttargetargs} /noshadow" -output:"#{coverage_file}"}

  rm 'TestResult.xml' if runner == :nunit

  coverage_file
end

def coverage(coverage_files)
  coverage_files = [*coverage_files]
  sh REPORT_GENERATOR, %Q{-reports:"#{coverage_files.join(';')}" "-targetdir:#{COVERAGE_DIR}"}
end


