import React from 'react';

function TravelBotTable({ topFiveCountries, onCountryClick }) {
    return (
        <table>
            <thead>
                <tr>
                    <th>Name</th>
                    <th>Capital</th>
                    <th>Population</th>
                    <th>Latitude</th>
                    <th>Longitude</th>
                </tr>
            </thead>
            <tbody>
                {topFiveCountries && topFiveCountries.map((country, index) => (
                    <tr key={index} onClick={() => onCountryClick(country.Name)}>
                        <td>{country.Name}</td>
                        <td>{country.Capital}</td>
                        <td>{country.Population}</td>
                        <td>{country.Latitude}</td>
                        <td>{country.Longitude}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    );
}


export default TravelBotTable;
