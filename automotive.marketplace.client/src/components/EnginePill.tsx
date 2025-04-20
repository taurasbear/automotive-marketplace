import { PiEngine } from "react-icons/pi";

const EnginePill = () => {
  return (
    <div className="border-primary-border flex w-full max-w-40 flex-row items-center justify-center gap-3 rounded-md border p-1">
      <PiEngine className="h-8 w-8" />
      <div className="flex flex-col w-28">
        <p className="font-sans text-sm">Engine</p>
        <p className="font-sans text-sm font-semibold">1,6 l 122 kW</p>
      </div>
    </div>
  );
};

export default EnginePill;
