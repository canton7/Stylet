CONFIG = ENV['CONFIG'] || 'Debug'

COVERAGE_DIR = 'Coverage'
COVERAGE_FILE = File.join(COVERAGE_DIR, 'coverage.xml')

GITLINK_REMOTE = 'https://github.com/canton7/stylet'
NUSPEC = 'NuGet/Stylet.nuspec'
NUSPEC_START = 'NuGet/Stylet.start.nuspec'

ASSEMBLY_INFO = 'Stylet/Properties/AssemblyInfo.cs'

CSPROJ = 'Stylet/Stylet.csproj'
MSBUILD = %q{C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe}

directory COVERAGE_DIR

desc "Build the project for release"
task :build do
  sh MSBUILD, CSPROJ, "/t:Clean;Rebuild", "/p:Configuration=Release", "/verbosity:normal"
end

task :test_environment => [:build] do
  NUNIT_TOOLS = 'packages/NUnit.Runners.*/tools'
  NUNIT_CONSOLE = Dir[File.join(NUNIT_TOOLS, 'nunit-console.exe')].first
  NUNIT_EXE = Dir[File.join(NUNIT_TOOLS, 'nunit.exe')].first

  OPENCOVER_CONSOLE = Dir['packages/OpenCover.*/tools/OpenCover.Console.exe'].first
  REPORT_GENERATOR = Dir['packages/ReportGenerator.*/tools/ReportGenerator.exe'].first

  UNIT_TESTS_DLL = "StyletUnitTests/bin/#{CONFIG}/StyletUnitTests.dll"
  INTEGRATION_TESTS_EXE = "StyletIntegrationTests/bin/#{CONFIG}/StyletIntegrationTests.exe"

  raise "NUnit.Runners not found. Restore NuGet packages" unless NUNIT_CONSOLE && NUNIT_EXE
  raise "OpenCover not found. Restore NuGet packages" unless OPENCOVER_CONSOLE
  raise "ReportGenerator not found. Restore NuGet packages" unless REPORT_GENERATOR
end

task :nunit_test_runner => [:test_environment] do
  sh NUNIT_CONSOLE, UNIT_TESTS_DLL
end

desc "Run unit tests using the current CONFIG (or Debug)"
task :test => [:nunit_test_runner] do |t|
  rm 'TestResult.xml', :force => true
end

desc "Launch the NUnit gui pointing at the correct DLL for CONFIG (or Debug)"
task :nunit => [:test_environment] do |t|
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
  sh OPENCOVER_CONSOLE, '-register:user', "-target:#{opttarget}", "-filter:+[Stylet]* -[Stylet]XamlGeneratedNamespace.*", "-targetargs:#{opttargetargs} /noshadow", "-output:#{coverage_file}"

  rm('TestResult.xml', :force => true) if runner == :nunit

  coverage_file
end

def coverage(coverage_files)
  coverage_files = [*coverage_files]
  sh REPORT_GENERATOR, "-reports:#{coverage_files.join(';')}", "-targetdir:#{COVERAGE_DIR}"
end

desc "Create NuGet package"
task :package do
  local_hash = `git rev-parse HEAD`.chomp
  sh "NuGet/GitLink.exe . -s #{local_hash} -u #{GITLINK_REMOTE} -f Stylet.sln -ignore StyletUnitTests,StyletIntegrationTests"
  Dir.chdir(File.dirname(NUSPEC)) do
    sh "nuget.exe pack #{File.basename(NUSPEC)}"
  end
  Dir.chdir(File.dirname(NUSPEC_START)) do
    sh "nuget.exe pack #{File.basename(NUSPEC_START)}"
  end
end

desc "Bump version number"
task :version, [:version] do |t, args|
  parts = args[:version].split('.')
  parts << '0' if parts.length == 3
  version = parts.join('.')

  content = IO.read(ASSEMBLY_INFO)
  content[/^\[assembly: AssemblyVersion\(\"(.+?)\"\)\]/, 1] = version
  content[/^\[assembly: AssemblyFileVersion\(\"(.+?)\"\)\]/, 1] = version
  File.open(ASSEMBLY_INFO, 'w'){ |f| f.write(content) }

  content = IO.read(NUSPEC)
  content[/<version>(.+?)<\/version>/, 1] = args[:version]
  File.open(NUSPEC, 'w'){ |f| f.write(content) }

  content = IO.read(NUSPEC_START)
  content[/<version>(.+?)<\/version>/, 1] = args[:version]
  content[%r{<dependency id="Stylet" version="\[(.+?)\]"/>}, 1] = args[:version]
  File.open(NUSPEC_START, 'w'){ |f| f.write(content) }
end

desc "Extract StyletIoC as a standalone file"
task :"extract-stylet-ioc" do
  filenames = Dir['Stylet/StyletIoC/**/*.cs']
  usings = Set.new
  files = []

  filenames.each do |file|
    contents = File.read(file)
    file_usings = contents.scan(/using .*?;$/)
    usings.merge(file_usings)
    
    matches = contents.match(/namespace (.+?)\n{\n(.+)}.*/m)
    namespace, file_contents = matches.captures

    files << {
      :from => file,
      :contents => file_contents,
      :namespace => namespace
    }
    # merged_contents << "    // Originally from #{file}\n\n" << file_contents << "\n"
  end

  File.open('StyletIoC.cs', 'w') do |outf|
    outf.write(usings.to_a.join("\n"))

    outf.puts

    files.group_by{ |x| x[:namespace ] }.each do |namespace, ns_files|
      outf.puts("\nnamespace #{namespace}")
      outf.puts("{")
      
      ns_files.each do |file|
        outf.puts("\n    // Originally from #{file[:from]}\n\n")
        outf.puts(file[:contents])
      end

      outf.puts("}\n")
    end
  end

  # puts merged_contents

end

