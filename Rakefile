CONFIG = ENV['CONFIG'] || 'Debug'

COVERAGE_DIR = 'Coverage'
COVERAGE_FILE = File.join(COVERAGE_DIR, 'coverage.xml')

GITLINK_REMOTE = 'https://github.com/canton7/stylet'
NUSPEC = 'NuGet/Stylet.nuspec'
NUSPEC_START = 'NuGet/Stylet.start.nuspec'

ASSEMBLY_INFO = 'Stylet/Properties/AssemblyInfo.cs'

CSPROJ = 'Stylet/Stylet.csproj'
UNIT_TESTS = 'StyletUnitTests/StyletUnitTests.csproj'

directory COVERAGE_DIR

desc "Build the project using the current CONFIG (or Debug)"
task :build do
  sh 'dotnet', 'build', '-c', CONFIG, CSPROJ
end

desc "Run unit tests using the current CONFIG (or Debug)"
task :test do
  sh 'dotnet', 'test', '-c', CONFIG, UNIT_TESTS
end

desc "Create NuGet package"
task :package do
  sh 'dotnet', 'pack', '-c', CONFIG, CSPROJ, "-p:NuSpecFile=../#{NUSPEC}"
  sh 'dotnet', 'pack', '-c', CONFIG, CSPROJ, "-p:NuSpecFile=../#{NUSPEC_START}"
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

