CONFIG = ENV['CONFIG'] || 'Debug'

COVERAGE_DIR = 'Coverage'
COVERAGE_FILE = File.join(COVERAGE_DIR, 'coverage.xml')

GITLINK_REMOTE = 'https://github.com/canton7/stylet'
NUSPEC = 'NuGet/Stylet.nuspec'
NUSPEC_START = 'NuGet/Stylet.start.nuspec'

ASSEMBLY_INFO = 'Stylet/Properties/AssemblyInfo.cs'

CSPROJ = 'Stylet/Stylet.csproj'
TEMPLATES_CSPROJ = 'StyletTemplates/StyletTemplates.csproj'
UNIT_TESTS = 'StyletUnitTests/StyletUnitTests.csproj'

TEMPLATES_DIR = 'StyletTemplates/templates'

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
  # Not sure why these have to be this way around, but they do
  sh 'dotnet', 'pack', '--no-build', '-c', CONFIG, CSPROJ, "-p:NuSpecFile=../#{NUSPEC_START}"
  sh 'dotnet', 'pack', '--no-build', '-c', CONFIG, CSPROJ, '-p:IncludeSymbols=true'
  sh 'dotnet', 'pack', '-c', CONFIG, TEMPLATES_CSPROJ
end

desc "Bump version number"
task :version, [:version] do |t, args|
  parts = args[:version].split('.')
  parts << '0' if parts.length == 3
  version = parts.join('.')

  content = IO.read(CSPROJ)
  content[/<VersionPrefix>(.+?)<\/VersionPrefix>/, 1] = version
  File.open(CSPROJ, 'w'){ |f| f.write(content) }

  content = IO.read(TEMPLATES_CSPROJ)
  content[/<VersionPrefix>(.+?)<\/VersionPrefix>/, 1] = version
  File.open(TEMPLATES_CSPROJ, 'w'){ |f| f.write(content) }

  content = IO.read(NUSPEC_START)
  content[/<version>(.+?)<\/version>/, 1] = args[:version]
  content[%r{<dependency id="Stylet" version="\[(.+?)\]"/>}, 1] = args[:version]
  File.open(NUSPEC_START, 'w'){ |f| f.write(content) }

  Dir[File.join(TEMPLATES_DIR, '**/*.csproj')].each do |csproj|
    content = IO.read(csproj)
    content[/<PackageReference Include="Stylet" Version="(.+?)" \/>/, 1] = version
    File.open(csproj, 'w'){ |f| f.write(content) }
  end
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

