import React, { useState } from 'react';
import TravelBotTable from './TravelBotTable';
import FetchTopFiveCountries from './FetchTopFiveCountries';

function TravelBot() {
    const [topFiveCountries, setTopFiveCountries] = useState([]);
    const [errorMessage, setErrorMessage] = useState('');

    function handleCountryClick(countryName) {
        fetch(`/api/Countries/summary?countryNames=${encodeURIComponent(countryName)}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Failed to fetch country summary');
                }
                return response.json();
            })
            .then(data => {
                console.log('Country Summary:', data);
            })
            .catch(error => {
                console.error('Error fetching country summary:', error.message);
                setErrorMessage('Failed to fetch country summary. Please try again later.');
            });
    }
    
    function handleFetchSuccess(data) {
        setTopFiveCountries(data);
        setErrorMessage('');
    }

    function handleFetchError(error) {
        console.error('Error fetching top five countries:', error);
        setErrorMessage('Failed to fetch top five countries. Please try again later.');
    }

    return (
        <div>
            <h2>Travel Bot</h2>
            {errorMessage && <p>{errorMessage}</p>}
            <h3>Top Five Countries by Population</h3>
            <FetchTopFiveCountries onSuccess={handleFetchSuccess} onError={handleFetchError} />
            <TravelBotTable topFiveCountries={topFiveCountries} onCountryClick={handleCountryClick} />
        </div>
    );
}

export default TravelBot;
