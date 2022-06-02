echo "build start"

cd fastlane

# rbenv
eval "$(rbenv init -)"

# pyenv
eval "$(pyenv init --path)"

bundle install

# fastlane
export LC_ALL=en_US.UTF-8
export LANG=en_US.UTF-8

echo "--- Building"
bundle exec fastlane ios upload_app_center job:export-ipa
echo "~~~ End Build"
