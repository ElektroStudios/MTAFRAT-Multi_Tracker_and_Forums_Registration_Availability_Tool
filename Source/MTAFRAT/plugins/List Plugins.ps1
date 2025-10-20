# Lista los campos Name y Url de todos los archivos JSON encontrados recursivamente.

$basePath = "."

$items = New-Object System.Collections.ArrayList

# Recorrer todos los JSON
Get-ChildItem -Path $basePath -Filter "*.json" -Recurse | ForEach-Object {
    try {
        $json = Get-Content $_.FullName -Raw | ConvertFrom-Json
        if ($json -isnot [System.Collections.IEnumerable]) { $json = @($json) }

        foreach ($item in $json) {
            if ($item.PSObject.Properties.Name -contains 'Name' -and $item.PSObject.Properties.Name -contains 'Url') {
                $null = $items.Add([PSCustomObject]@{
                    Índice = (++$i).ToString("00")
                    Nombre = $item.Name
                    Url  = $item.Url
                })
            }
        }
    } catch {
        Write-Warning "Error procesando $($_.FullName)"
    }
}

$items | Format-Table -AutoSize Índice, Nombre, Url

Write-Host ""
Pause
Exit /B 0
