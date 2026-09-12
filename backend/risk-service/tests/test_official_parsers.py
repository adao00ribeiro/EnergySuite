"""
Unit tests for official ONS/CCEE sector parsers (DADVAZ, PREVS, VNA).
"""

import sys
import os
import unittest

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '..')))

from src.official_parsers import (
    parse_dadvaz, generate_dadvaz,
    parse_prevs, generate_prevs,
    parse_vna, generate_vna
)


class TestOfficialParsers(unittest.TestCase):

    def test_dadvaz_parse_and_generate(self):
        sample_records = [
            {"usina_id": 1, "time_step": "SEM-1", "flow_m3s": 1250.50},
            {"usina_id": 6, "time_step": "SEM-2", "flow_m3s": 840.00}
        ]
        
        generated_content = generate_dadvaz(sample_records)
        self.assertIn("DADVAZ", generated_content)
        self.assertIn("1250.50", generated_content)
        
        parsed = parse_dadvaz(generated_content)
        self.assertEqual(len(parsed), 2)
        self.assertEqual(parsed[0]["usina_id"], 1)
        self.assertEqual(parsed[0]["flow_m3s"], 1250.50)

    def test_prevs_parse_and_generate(self):
        sample_records = [
            {"posto_id": 1, "period": "SEM-1", "ena_mwmed": 4500.0},
            {"posto_id": 1, "period": "SEM-2", "ena_mwmed": 4800.0}
        ]
        
        generated_content = generate_prevs(sample_records)
        self.assertIn("POSTO 1", generated_content)
        self.assertIn("4500.00", generated_content)
        
        parsed = parse_prevs(generated_content)
        self.assertEqual(len(parsed), 2)
        self.assertEqual(parsed[0]["posto_id"], 1)
        self.assertEqual(parsed[0]["ena_mwmed"], 4500.0)

    def test_vna_parse_and_generate(self):
        sample_records = [
            {"date": "2026-09-01", "vna_value": 4125.678900},
            {"date": "2026-09-02", "vna_value": 4126.123450}
        ]
        
        generated_content = generate_vna(sample_records)
        self.assertIn("2026-09-01", generated_content)
        self.assertIn("4125.678900", generated_content)
        
        parsed = parse_vna(generated_content)
        self.assertEqual(len(parsed), 2)
        self.assertEqual(parsed[0]["date"], "2026-09-01")
        self.assertEqual(parsed[0]["vna_value"], 4125.678900)


if __name__ == '__main__':
    unittest.main()
