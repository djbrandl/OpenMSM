import React, { Component } from 'react';
import { connect } from 'react-redux';
import { Collapse, Card, CardTitle, Row, Col, Button } from 'reactstrap';

class LastApiCall extends Component {
    render() {
        return (
            <div>
                <p>To see the details of the web calls performed, click the button below to show/hide the API call details.</p>
                <Button onClick={this.props.toggleApi}>Toggle API details</Button>
                <Collapse isOpen={this.props.lastApiCall.showApi}>
                    <Row>
                        <Col sm="6">
                            <Card body>
                                <CardTitle>{this.props.lastApiCall.url}</CardTitle>
                                <pre>
                                    {JSON.stringify(this.props.lastApiCall.details, undefined, 2)}
                                </pre>
                            </Card>
                        </Col>

                        <Col sm="6">
                            <Card body>
                                <CardTitle>Response</CardTitle>
                                <pre>
                                    {JSON.stringify(this.props.lastApiCall.response, undefined, 2)}
                                </pre>
                            </Card>
                        </Col>
                    </Row>
                </Collapse>
            </div>
        );
    }
}

export default connect()(LastApiCall);
