import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import Home from './Home'; 
import TravelBot from './TravelBot'; 

class Layout extends Component {
    render() {
        return (
            <div>
                <NavMenu />
                <Container>
                    {this.props.children || <Home />} 
                    <TravelBot /> 
                </Container>
            </div>
        );
    }
}

export default Layout; 
