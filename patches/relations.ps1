cd ..
Get-ChildItem -Directory Schemas | ForEach-Object {
    $dir = $_
    Get-ChildItem -File patches/*.patch | ForEach-Object { git apply "patches/$($_.Name)" --directory "Schemas/$($dir.Name)" }
}
cd patches