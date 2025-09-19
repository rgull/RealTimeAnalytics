import { Box } from "@mui/material";
import Dashboard from "./components/Dashboard";
import { SignalRProvider } from "./services/SignalRService";

function App() {
  return (
    <SignalRProvider>
      <Box sx={{ minHeight: "100vh" }}>
        <Dashboard />
      </Box>
    </SignalRProvider>
  );
}

export default App;
