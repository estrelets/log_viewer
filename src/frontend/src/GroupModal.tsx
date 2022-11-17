import {
    ActionIcon,
    Badge,
    Code,
    Container,
    Group,
    Modal,
    Table,
    ThemeIcon,
    Timeline,
    TimelineItem,
    Text, MantineColor
} from "@mantine/core";
import React, {useState} from "react";
import {IconMaximize, IconBug, IconInfoCircle} from "@tabler/icons";
import {GroupEntry, LogEntry} from "./Api";

function GroupModal(data: { group: GroupEntry }) {
    const [opened, setOpened] = useState(false);

    const mapBullet = (type: number) => type == 0 
        ? <ThemeIcon color={'red'} radius={'xl'}><IconBug/></ThemeIcon>    
        : <ThemeIcon radius={'xl'}><IconInfoCircle/></ThemeIcon>;

    const mapTitleColor  = (log: LogEntry): MantineColor => {
        if (log.duration < 75) return 'green';
        if(log.duration < 150) return 'yellow'
        return 'red';
    };
    
    const mapTitle = (log: LogEntry) => (
        <Group>
            <Text> {log.time.substring(14)}</Text>
            <Badge color={mapTitleColor(log)}>
                {log.duration}
            </Badge>
        </Group>
    );
    
    const items = data.group.logs.map(log => (
        <TimelineItem 
            title={mapTitle(log)}
            bullet={mapBullet(log.type)}
            bulletSize={24}
            color={'red'}>
            <Container fluid>
                <Code block color={log.type == 0 ? 'red' : 'none'}>
                    {log.messagePreview}
                </Code>
            </Container>
        </TimelineItem>
    ))
    
    
    
    return (
        <>
            <Modal
                size={'100%'}  withCloseButton={false}  
                opened={opened}
                onClose={() => setOpened(false)}
            >
                <Timeline>
                    {items}
                </Timeline>
            </Modal>

            <ActionIcon onClick={() => setOpened(true)}>
                <IconMaximize></IconMaximize>
            </ActionIcon>
        </>
    )
}

export default GroupModal;