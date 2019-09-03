COMPILE_TARGET = ENV['config'].nil? ? "debug" : ENV['config']
RESULTS_DIR = "results"
BUILD_VERSION = '3.1.0'

tc_build_number = ENV["BUILD_NUMBER"]
build_revision = tc_build_number || Time.new.strftime('5%H%M')
build_number = "#{BUILD_VERSION}.#{build_revision}"
BUILD_NUMBER = build_number

task :ci => [:default, :pack]

task :default => [:test]

desc "Prepares the working directory for a new build"
task :clean do
	#TODO: do any other tasks required to clean/prepare the working directory
	FileUtils.rm_rf RESULTS_DIR
	FileUtils.rm_rf 'artifacts'

end



desc 'Compile the code'
task :compile => [:clean] do
	sh "dotnet restore Lamar.sln"
end

desc 'Run the unit tests'
task :test => [:compile] do
	Dir.mkdir RESULTS_DIR

	sh "dotnet test src/LamarCompiler.Testing/LamarCompiler.Testing.csproj"
	sh "dotnet test src/Lamar.Testing/Lamar.Testing.csproj"
	sh "dotnet test src/Lamar.AspNetCoreTests/Lamar.AspNetCoreTests.csproj"
end

desc "Pack up the nupkg file"
task :pack => [:compile] do
    sh "dotnet pack src/LamarCodeGeneration/LamarCodeGeneration.csproj -o ./../../artifacts --configuration Release"
	sh "dotnet pack src/LamarCompiler/LamarCompiler.csproj -o ./../../artifacts --configuration Release"
	sh "dotnet pack src/Lamar/Lamar.csproj -o ./../../artifacts --configuration Release"
	sh "dotnet pack src/Lamar.Diagnostics/Lamar.Diagnostics.csproj -o ./../../artifacts --configuration Release"
	sh "dotnet pack src/Lamar.Microsoft.DependencyInjection/Lamar.Microsoft.DependencyInjection.csproj -o ./../../artifacts --configuration Release"
end

# TODO -- redo these tasks
desc "Launches VS to the Lamar solution file"
task :sln do
	sh "start Lamar.sln"
end


"Launches the documentation project in editable mode"
task :docs do
	sh "dotnet restore docs.csproj"
	sh "dotnet stdocs run -v #{BUILD_VERSION}"
end

"Exports the documentation to jasperfx.github.io - requires Git access to that repo though!"
task :publish do
	FileUtils.remove_dir('doc-target') if Dir.exists?('doc-target')

	if !Dir.exists? 'doc-target'
		Dir.mkdir 'doc-target'
		sh "git clone -b gh-pages https://github.com/jasperfx/lamar.git doc-target"
	else
		Dir.chdir "doc-target" do
			sh "git checkout --force"
			sh "git clean -xfd"
			sh "git pull origin master"
		end
	end

	sh "dotnet restore docs.csproj"
	sh "dotnet stdocs export doc-target ProjectWebsite --version #{BUILD_VERSION} --project lamar"

	Dir.chdir "doc-target" do
		sh "git add --all"
		sh "git commit -a -m \"Documentation Update for #{BUILD_VERSION}\" --allow-empty"
		sh "git push origin gh-pages"
	end




end
