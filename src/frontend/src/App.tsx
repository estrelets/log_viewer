import React, {useEffect, useState} from 'react';
import {Button, Container, MantineProvider, Pagination, FileButton, Center, Group, TextInput} from "@mantine/core";
import './App.css';
import {GetGroups, UploadLogFile, GroupEntry} from "./Api";
import GroupsTable from "./GroupsTable";
import {useDebouncedState, useDebouncedValue, useHotkeys} from "@mantine/hooks";

function App() {
    const [groups, setGroups] = useState<GroupEntry[]>([]);
    const [activePage, setPage] = useState(1);
    const [searchText, setSearchText] = useDebouncedState('', 200);
    const [pageCount, setPageCount] = useState(20);

    useHotkeys([
        ['ArrowLeft', () => setPage(activePage -1)],
        ['ArrowRight', () => setPage(activePage +1)],
    ]);

    useEffect(() => {
        GetGroups(activePage, searchText, setGroups, setPageCount);
    }, [activePage, searchText]);

    useEffect(() => {
        setPage(1);
    }, [searchText]);

    return (
        <MantineProvider withGlobalStyles withNormalizeCSS>
            <Container fluid>
                <Center>
                    <Group>
                        <Pagination page={activePage} onChange={setPage} total={pageCount} siblings={3} initialPage={10}/>
                        <TextInput
                            defaultValue={searchText}
                            onChange={e => setSearchText(e.currentTarget.value)}
                            variant={"filled"} radius={"xl"} size={"md"} placeholder={"text"} />
                    </Group>
                </Center>

                { groups && <GroupsTable groups={groups} setGroups={setGroups}/> }

                <UploadLogs/>
            </Container>
        </MantineProvider>
    );
}

function UploadLogs() {
    return (
        <FileButton onChange={UploadLogFile}>
            {(props) => <Button {...props}>Upload logs</Button>}
        </FileButton>
    )
}

export default App;
